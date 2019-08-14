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
    Button comfirmBtn;

    [SerializeField]
    Button[] setLevelBtns;

    //[SerializeField]
    //ColorBlock[] LevelColorBlocks;

    [SerializeField]
    private Dictionary<int, GameObject> levelUIs = new Dictionary<int, GameObject>();
    private int idCounter = 0;
    private int idFocus;

    private LevelItem selectOne = null;

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
                    case "GetAllPoint":
                        GetAllPoint(data.GetData<List<Level>>());
                        break;
                    default:
                        Debug.Log(data.action);
                        break;
                }
            }
        };
        var items = GetComponentsInChildren<LevelItem>();
        for (int i = 0; i < items.Length; i++)
        {
            var e = items[i];
            e.pointerDown = ()=> { Focus(e.level.id); } ;
            levelUIs[e.level.id] = items[i].gameObject;
        }
        

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
                thisLevelUI.GetComponent<LevelItem>().level = level;
                RenderLevelUI(ref thisLevelUI, level.id, Utility.ParseEnum<TeamName>(level.belong));
            }
        });
    }

    // ComfirmBtn Onclick()
    public void Comfirm()
    {
        ResetFocus();
        // Turn off LeveUI
        if (selectOne!=null)
        {
            SendToSocket("UpdatePoint", selectOne.level);
        }
        //foreach (var levelUI in levelUIs)
        //{
        //    LevelItem LeveUI = levelUI.Value.GetComponent<LevelItem>();
        //    LeveUI.active = false;
        //    SendToSocket("UpdatePoint", LeveUI.level);
        //}
        if (curPageStatus == PageStatus.Idle)
        {
            return;
        }
        ChangePage(PageStatus.Idle);
    }

    private void ResetFocus()
    {
        Focus(-1);
    }

    public void Focus(int id)
    {
        if (curPageStatus != PageStatus.Idle) return;
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
            selectOne = levelUI.GetComponent<LevelItem>();
        }
        EnableLevelSubPage(valid);
    }

    Color[] colorArray = new Color[5] { new Color(1,1,1, 0.7f), new Color(1, 0.3349057f, 0.3349057f, 0.7f), new Color(0.514151f, 0.7544931f, 1, 0.7f), new Color(0.07906572f, 0.754717f, 0, 0.7f), new Color(1, 0.9381177f, 0.2688679f, 0.7f) };

    // setLevelBtns Onclick()
    public void SetLevelBelong(int index)
    {
        if (localLevels.TryGetValue(idFocus, out Level drityLevel))
        {
            drityLevel.belong = ((TeamName)index).ToString();
            var ui = levelUIs[drityLevel.id];
            ui.GetComponent<Image>().color = colorArray[index];
            ui.GetComponentInChildren<Text>().text = drityLevel.belong;
            ChangePage(PageStatus.Edit);
        }
    }

    void EnableLevelSubPage(bool active)
    {
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
                comfirmBtn.transform.gameObject.SetActive(false);
                EnableLevelSubPage(false);
                break;
            case PageStatus.Edit:
                comfirmBtn.transform.gameObject.SetActive(true);
                break;
        }
        curPageStatus = pageStatus;
    }

    void RenderLevelUI(ref GameObject instance, int id, TeamName belong)
    {
        Text text = instance.GetComponentInChildren<Text>();
        text.text = belong.ToString();
        bool isTeamIndexVaild = belong >= 0 && (int)belong < 5;
        if (isTeamIndexVaild)
        {
            instance.GetComponent<Image>().color = colorArray[(int)belong];
        }
    }

    void UpdateLevels(Level level)
    {
        update.Push(() =>
        {
            var l = localLevels[level.id];
            if (l != null)
            {
                var obj = levelUIs[l.id];
                l.belong = level.belong;
                RenderLevelUI(ref obj, l.id, Utility.ParseEnum<TeamName>(l.belong));
            }
        });
    }

    //int GetTeamIndexByName(string name)
    //{
    //    int teamIndex = -1;
    //    for (int i = 0; i < belongTeamName.Length; ++i)
    //    {
    //        if (name == belongTeamName[i])
    //        {
    //            teamIndex = i;
    //            break;
    //        }
    //    }
    //    return teamIndex;
    //}

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