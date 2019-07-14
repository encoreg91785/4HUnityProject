using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelBoardUI : UIDialog
{
    public GameObject levelUI = null;
    Stack<Action> update = new Stack<Action>();
    Stack<Action> send = new Stack<Action>();
    [SerializeField]
    Button editBtn, comfirmBtn, addBtn, deleteBtn;

    [SerializeField]
    Button[] setLevelBtns;

    [SerializeField]
    ColorBlock[] LevelColorBlocks;

    [SerializeField]
    private Dictionary<int, GameObject> levelUIs = new Dictionary<int, GameObject>();
    private int idCounter = 0;
    private int idFocus;

    private readonly string[] belongTeamName = { "隊伍1", "隊伍2", "隊伍3", "隊伍4", "" };

    private Dictionary<int, Level> localLevels = new Dictionary<int, Level>();

    enum PageStatus
    {
        Idle,
        Edit,
    }
    PageStatus curPageStatus = PageStatus.Idle;


    private void Start()
    {
        //強連線
        SocketConnection.Instance.GetDataFunction = (s) =>
        {
            var data = JsonConvert.DeserializeObject<SocketData>(s);
            if (data != null)
            {
                switch (data.action)
                {
                    case "UpdatePoint":
                        UpdateLevels(data.GetData<Level>());
                        break;
                    case "DeletePoint":
                        DeletePoint(data.GetData<int>());
                        break;
                    case "AddPoint":
                        GetPoint(data.GetData<Level>());
                        break;
                    case "GetAllPoint":
                        GetAllPoint(data.GetData<List<Level>>());
                        break;
                    default:
                        Debug.Log(data.action);
                        break;
                }
            }
                
        };

        SocketConnection.Instance.ConnectToTcpServer(Main.GetInstance().ip, 1337);
        new Promise().Then(_ =>
        {
            var connect = SocketConnection.Instance;
            return Answer.PendingUntil(() => { return connect.ConnectionIsAlive; });
        }).Then(_ =>
        {
            SendToSocket("GetAllPoint", null);
            return Answer.Resolve();
        }).Invoke(this);
    }

    private void Update()
    {
        while (update.Count > 0)
        {
            update.Pop()();
        }
        if (send.Count > 0)
        {
            send.Pop()();
        }
        //while (send.Count > 0)
        //{
        //    send.Pop()();
        //}
    }

    private void OnDestroy()
    {
        SocketConnection.Instance.DisconnectToTcpServer();
    }

    // 模擬Server回傳
    void GetAllPoint(List<Level> ls)
    {
        update.Push(() =>
        {
            foreach (var level in ls)
            {
                localLevels[level.id] =level;
                // Render all LevelUIs except focus one
                GameObject thisLevelUI = null;
                levelUIs.TryGetValue(level.id, out thisLevelUI);
                RenderLevelUI(ref thisLevelUI, level.id, level.position, level.belong);
            }
        });
    }

    // EditBtn Onclick()
    public void Edit()
    {
        idFocus = -1;
        if (curPageStatus == PageStatus.Edit)
        {
            return;
        }
        foreach (var item in levelUIs)
        {
            item.Value.GetComponent<LevelItem>().active = true;
        }
        ChangePage(PageStatus.Edit);
    }

    // ComfirmBtn Onclick()
    public void Comfirm()
    {
        ResetFocus();
        // Turn off LeveUI
        foreach (var levelUI in levelUIs)
        {
            LevelItem LeveUI = levelUI.Value.GetComponent<LevelItem>();
            LeveUI.active = false;
            SendToSocket("UpdatePoint", LeveUI.level);
        }
        if (curPageStatus == PageStatus.Idle)
        {
            return;
        }
        ChangePage(PageStatus.Idle);
    }

    // AddBtn Onclick()
    public void Add()
    {
        Level level = new Level
        {
            id = 0,
            position = new Vector3(0, 0, 0),
            belong = belongTeamName[0]
        };
        SendToSocket("AddPoint", level);
        // 模擬Server指令 AddPoint
    }

    private void ResetFocus()
    {
        Focus(-1);
    }

    public void Focus(int id)
    {
        if (curPageStatus != PageStatus.Edit) return;
        // Beforce changing
        if (levelUIs.TryGetValue(idFocus, out GameObject levelUI))
        {
            levelUI.GetComponent<LevelItem>().SetFocus(false);
        }
        idFocus = id;
        // After changing
        bool valid = levelUIs.TryGetValue(idFocus, out levelUI);
        if (valid)
        {
            levelUI.GetComponent<LevelItem>().SetFocus(true);
        }
        EnableLevelSubPage(valid);
    }

    Color[] colorArray = new Color[5] { new Color(1, 0.3349057f, 0.3349057f), new Color(0.514151f, 0.7544931f, 1), new Color(0.6307352f,1, 0.5896226f), new Color(1, 0.9381177f, 0.2688679f), Color.white };

    // setLevelBtns Onclick()
    public void SetLevelBelong(int teamIndex)
    {
        if (localLevels.TryGetValue(idFocus, out Level drityLevel))
        {
            drityLevel.belong = belongTeamName[teamIndex];
            var ui = levelUIs[drityLevel.id];
            ui.GetComponent<Image>().color = colorArray[teamIndex];
        }
    }

    void GetPoint(Level l)
    {
        update.Push(() =>
        {
            localLevels[l.id] = l;
            GameObject obj = null;
            levelUIs.TryGetValue(l.id, out obj);
            RenderLevelUI(ref obj, l.id, l.position, l.belong);
        });
    }

    // deleteBtn OnClick()
    public void Delete()
    {
        SendToSocket("DeletePoint", idFocus);
        // 模擬Server指令 DeletePoint
        ResetFocus();
    }

    void EnableLevelSubPage(bool active)
    {
        deleteBtn.transform.gameObject.SetActive(active);
        foreach (var btn in setLevelBtns)
        {
            btn.transform.gameObject.SetActive(active);
        }
    }

    void ChangePage(PageStatus pageStatus)
    {
        switch (pageStatus)
        {
            case PageStatus.Idle:
                editBtn.transform.gameObject.SetActive(true);
                comfirmBtn.transform.gameObject.SetActive(false);
                addBtn.transform.gameObject.SetActive(false);
                break;
            case PageStatus.Edit:
                editBtn.transform.gameObject.SetActive(false);
                comfirmBtn.transform.gameObject.SetActive(true);
                addBtn.transform.gameObject.SetActive(true);
                break;
        }
        curPageStatus = pageStatus;
    }

    void DeletePoint(int id)
    {
        update.Push(() =>
        {
            localLevels.Remove(id);
            var obj = levelUIs[id];
            if (levelUIs != null)
            {
                Destroy(obj);
                levelUIs.Remove(id);
            }
        });
    }

    void RenderLevelUI(ref GameObject instance, int id, Vector3 position, string belong)
    {
        LevelItem LeveUI = null;
        if (instance == null)
        {
            instance = Instantiate(levelUI);
            LeveUI = instance.GetComponent<LevelItem>();
            LeveUI.SetFocus(false);
            LeveUI.pointerDown = () => { Focus(id); };
            LeveUI.level = localLevels[id];
        }
        Text text = instance.GetComponentInChildren<Text>();
        text.text = "關卡" + id;
        RectTransform rectTransform = instance.GetComponent<RectTransform>();
        rectTransform.transform.SetParent(transform);
        rectTransform.position = position;
        rectTransform.localScale = Vector3.one;
        if (curPageStatus == PageStatus.Edit)
        {
            LeveUI.active = true;
        }
        int teamIndex = GetTeamIndexByName(belong);
        bool isTeamIndexVaild = teamIndex >= 0 && teamIndex < belongTeamName.Length;
        if (isTeamIndexVaild)
        {
            instance.GetComponent<Image>().color = colorArray[teamIndex];
        }
        levelUIs[id] = instance;
    }

    void UpdateLevels(Level level)
    {
        update.Push(() =>
        {
            var l = localLevels[level.id];
            if (l != null)
            {
                var obj = levelUIs[l.id];
                l.position = level.position;
                l.belong = level.belong;
                RenderLevelUI(ref obj, l.id, l.position, l.belong);
            }
        });
    }

    int GetTeamIndexByName(string name)
    {
        int teamIndex = -1;
        for (int i = 0; i < belongTeamName.Length; ++i)
        {
            if (name == belongTeamName[i])
            {
                teamIndex = i;
                break;
            }
        }
        return teamIndex;
    }

    void SendToSocket(string action , object data)
    {
        send.Push(() =>
        {
            var msg = JsonConvert.SerializeObject(new { action = action, data = data });
            SocketConnection.Instance.SentDataToServer(msg);
        });
    }

    public void Return()
    {
        UIManager.GetInstance().OpenDialog("MainMenuUI");
        UIManager.GetInstance().CloseDialog(this);
    }
}