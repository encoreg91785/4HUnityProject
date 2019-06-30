using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelBoardUI : UIDialog
{
    public GameObject levelUI = null;

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

    private readonly string[] belongTeamName = { "隊伍1", "隊伍2", "隊伍3", "隊伍4" };

    private Dictionary<int, Level> localLevels = new Dictionary<int, Level>();
    private Queue<Level> dirtyLevels = new Queue<Level>();

    enum PageStatus
    {
        Idle,
        Edit,
    }
    PageStatus curPageStatus = PageStatus.Idle;
    

    private void Start()
    {
        /*SocketConnection.Instance.GetDataFunction = (s) =>
        {
            Debug.Log(s);
        };
        SocketConnection.Instance.ConnectToTcpServer("61.230.66.173", 1337);

        SocketConnection.Instance.SentDataToServer(JsonConvert.SerializeObject(
                new
                {
                    action = "GetAllPoint",
                    data = new { id = 1, position = new int[] { 1, 2, 3 }, belong = "a" }
                }));*/
    }

    private void OnDestroy()
    {
        //SocketConnection.Instance.DisconnectToTcpServer();
    }

    // 模擬Server回傳
    List<Level> serverLevels = new List<Level>();
    void Update()
    {
        Dictionary<int, Level> newLocalLevels = new Dictionary<int, Level>();
        foreach (var level in serverLevels)
        {
            newLocalLevels.Add(level.id, level);

            // Render all LevelUIs except focus one
            levelUIs.TryGetValue(level.id, out GameObject thisLevelUI);
            if (idFocus != level.id)
            {
                RenderLevelUI(ref thisLevelUI, level.id, level.position, level.belong);
            }
        }
        localLevels = newLocalLevels;
    }

    // EditBtn Onclick()
    public void Edit()
    {
        idFocus = -1;
        if (curPageStatus == PageStatus.Edit)
        {
            return;
        }
        ChangePage(PageStatus.Edit);
    }

    // ComfirmBtn Onclick()
    public void Comfirm()
    {
        UpdateDirtyLevels();
        ResetFocus();
        // Turn off LeveUI
        foreach (var levelUI in levelUIs)
        {
            LevelItem LeveUI = levelUI.Value.GetComponent<LevelItem>();
            LeveUI.active = false;
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
            id = ++idCounter,
            position = new Vector3(0, 0, 0),
            belong = belongTeamName[0]
        };
        localLevels.Add(level.id, level);
        // 模擬Server指令 AddPoint
        serverLevels.Add(level);
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
        if ( valid )
        {
            levelUI.GetComponent<LevelItem>().SetFocus(true);
        }
        EnableLevelSubPage(valid);

        // Mark focus level dirty
        if (localLevels.TryGetValue(idFocus, out Level drityLevel))
        {
            dirtyLevels.Enqueue(drityLevel);
        }
    }

    // setLevelBtns Onclick()
    public void SetLevelBelong(int teamIndex)
    {
        if (localLevels.TryGetValue(idFocus, out Level drityLevel))
        {
            drityLevel.belong = belongTeamName[teamIndex];
        }
    }

    // deleteBtn OnClick()
    public void Delete()
    {
        levelUIs.TryGetValue(idFocus, out GameObject levelUI);
        levelUIs.Remove(idFocus);
        Destroy(levelUI);

        localLevels.Remove(idFocus);

        // 模擬Server指令 DeletePoint
        for (int i = 0; i < serverLevels.Count; ++i)
        {
            if (serverLevels[i].id == idFocus)
            {
                serverLevels.RemoveAt(i);
                break;
            }
        }
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

    void RenderLevelUI(ref GameObject instance, int id, Vector3 position, string belong)
    {
        if (instance == null)
        {
            instance = Instantiate(levelUI);
        }
        Text text = instance.GetComponentInChildren<Text>();
        text.text = "關卡" + id;
        RectTransform rectTransform = instance.GetComponent<RectTransform>();
        rectTransform.transform.SetParent(transform);
        rectTransform.localPosition = position;
        rectTransform.localScale = Vector3.one;
        if (curPageStatus == PageStatus.Edit)
        {
            LevelItem LeveUI = instance.GetComponent<LevelItem>();
            LeveUI.active = true;
            LeveUI.pointerDown = () => { Focus(id); };
            LeveUI.level = localLevels[id];
        }
        Button btn = instance.GetComponent<Button>();
        int teamIndex = GetTeamIndexByName(belong);
        bool isTeamIndexVaild = teamIndex >= 0 && teamIndex < belongTeamName.Length;
        if (isTeamIndexVaild)
        {
            btn.colors = LevelColorBlocks[teamIndex];
        }
        levelUIs[id] = instance;
    }

    void UpdateDirtyLevels()
    {
        while (dirtyLevels.Count > 0)
        {
            Level level = dirtyLevels.Dequeue();
            // 模擬Server指令 UpdatePoint
            for (int i = 0; i < serverLevels.Count; ++i)
            {
                if (level.id == serverLevels[i].id)
                {
                    serverLevels[i].position = level.position;
                    serverLevels[i].belong = level.belong;
                }
            }
        }
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
}