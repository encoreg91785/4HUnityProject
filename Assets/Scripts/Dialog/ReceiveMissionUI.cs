using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ReceiveMissionUI : UIDialog
{
    [SerializeField]
    Text taskName, taskCondition, taskInformation;
    [SerializeField]
    Button confirmBtn,scanPlayerBtn;
    [SerializeField]
    PlayerItem playerItem;
    [SerializeField]
    ScrollRect playerListView;
    List<PlayerItem> itemList = new List<PlayerItem>();
    List<Player> playerList = new List<Player>();
    TaskData task = null;
    private void Start()
    {
        CleanUI();
    }


    public void Return()
    {
        UIManager.GetInstance().OpenDialog("MainMenuUI");
        UIManager.GetInstance().CloseDialog(this);
    }

    public void ScanPlayerQRCode()
    {
        var ui = Main.GetInstance().OpenQRCodeUI();
        ui.resultAction = GetPlayerQRCode;
        ui.StartScan();
    }

    public void ScanTaskQRCode()
    {
        QRCodeUI ui = Main.GetInstance().OpenQRCodeUI();
        ui.resultAction = GetTaskQRCode;
        ui.StartScan();
    }

    void GetPlayerQRCode(string qrcode)
    {
        CheckPlayerTaskCount(qrcode);
    }

    void GetTaskQRCode(string qrcode)
    {
        GetTaskData(qrcode);
        Main.GetInstance().CloseUIQRCode();
    }

    void GetTaskData(string qrcode)
    {
        UIManager.GetInstance().OpenDialog("LoadingUI");
        new Promise().Then(_ => {
            task = Main.GetInstance().GetTaskData(qrcode);
            if (task != null)
            {
                
                UnityWebRequest www = HttpHelper.DoGet("temp/taskAndcard/count", new { taskqrcode = task.qrcode, cardid = task.cardid });
                return Answer.Resolve(www);
            }
            else
            {
                return Answer.Reject(qrcode+" is null");
            }
                
        }).Then(result => {
            return Utility.ParseServerRespond<int[]>((string)result);
        }).Then(result => {
            var r = result as int[];
            if (CheckTaskCount(r[0]))
            {
                taskName.text = task.name;
                taskCondition.text = task.condition;
                taskInformation.text = task.information;
                confirmBtn.interactable = playerList.Count > 0;
                scanPlayerBtn.interactable = true;
                return Answer.Resolve();
            }
            else
            {
                confirmBtn.interactable = false;
                return Answer.Reject(string.Format("任務{0}/{1}", r[0], task.max));
            }
        }).Done(() => {
            UIManager.GetInstance().CloseDialog("LoadingUI");
        }).Reject(error => {
            Main.GetInstance().CloseUIQRCode();
            var ui = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
            ui.SetUI(error,true);
            Debug.Log(error);
        }).Invoke(this);
    }

    public void CleanUI()
    {
        taskName.text = "";
        taskCondition.text = "";
        taskInformation.text = "";
        task = null;
        playerList.Clear();
        confirmBtn.interactable = false;
        scanPlayerBtn.interactable = false;
        for (int i = 0; i < itemList.Count; i++)
        {
            Destroy(itemList[i].gameObject);
        }
        itemList.Clear();
    }

    void GetPlayerData(string qrcode)
    {
        new Promise().Then(_ => {
            UnityWebRequest www = HttpHelper.DoGet("player", new { playerqrcode = qrcode });
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<Player>((string)result);
        }).Then(result => {
            var p  = result as Player;
            var pl = playerList.Find(e => { return e.qrcode == p.qrcode; });
            //檢查是否重複掃描了
            
            if (pl == null)
            {
                playerList.Add(p);
                string msg = "";
                for (int i = 0; i < playerList.Count; i++)
                {
                    msg += "名稱: " + playerList[i].name + "\n";
                }
                msg += "是否繼續掃描?";
                var u = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
                QRCodeUI qrUi = (QRCodeUI)UIManager.GetInstance().GetDialog("QRCodeUI");
                qrUi.StopScan();
                Action enter = () => { qrUi.StartScan(); };
                Action cancel = () => { Main.GetInstance().CloseUIQRCode(); };
                u.SetUI(msg, false, enter, cancel);
                AddPlayerItem(p);
                confirmBtn.interactable = task != null;
            }
            else
            {
                var u = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
                u.SetUI(pl.name+"已經掃描過了",true, ()=> {
                    QRCodeUI qrUi = (QRCodeUI)UIManager.GetInstance().GetDialog("QRCodeUI");
                    qrUi.StartScan();
                });
            }
            return Answer.Resolve();
        }).Done(()=> {
            UIManager.GetInstance().CloseDialog("LoadingUI");
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
    }

    void CheckPlayerTaskCount(string qrcode)
    {
        new Promise().Then(_ => {
            UIManager.GetInstance().OpenDialog("LoadingUI");
            UnityWebRequest www = HttpHelper.DoGet("task/count", new { playerqrcode = qrcode, taskqrcode=task.qrcode });
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<int>((string)result);
        }).Then(result => {
            if ((int)result > 0)
            {
                QRCodeUI qrUi = (QRCodeUI)UIManager.GetInstance().GetDialog("QRCodeUI");
                qrUi.StopScan();
                Action enter = () => { GetPlayerData(qrcode); };
                Action cancel = () => {
                    qrUi.StartScan();
                    UIManager.GetInstance().CloseDialog("LoadingUI");
                };
                var ui = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
                ui.SetUI(string.Format("此玩家已完成過{0}次任務，是否讓他加入此任務?", (int)result),false,enter,cancel);
            }
            else
            {
                GetPlayerData(qrcode);
            }
            return Answer.Resolve();
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
    }

    void AddPlayerItem(Player p)
    {
        var obj = Instantiate(playerItem, playerListView.content);
        obj.gameObject.SetActive(true);
        obj.SetPalyer(p);
        itemList.Add(obj);
    }

    public void OnConfimTask()
    {
        
        List<string> qrcode = new List<string>();
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].IsSelect()) qrcode.Add(itemList[i].player.qrcode);
        }
        int[] tc =new int[0];
        Utility.LoadingPromise().Then(result => {
            UnityWebRequest www = HttpHelper.DoGet("temp/taskAndcard/count", new {cardid = task.cardid, taskqrcode = task.qrcode });
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<int[]>((string)result);
        }).Then(result => {
            tc = result as int[];
            if (CheckTaskCount(tc[0]+ qrcode.Count))
            {
                UnityWebRequest www = HttpHelper.DoPost("task", new { playerqrcode = qrcode, taskqrcode = task.qrcode });
                return Answer.Resolve(www);
            }
            else
            {
                return Answer.Reject(string.Format("({0}+{1})/{2}\n{3} 任務將超過可完成上限，請確認人數", tc[0],qrcode.Count,task.max,task.name));
            }
        }).Then(result => {
            return Utility.ParseServerRespond<object>((string)result);
        }).Then(result => {
            var carddata = Main.GetInstance().GetCardData(task.cardid);
            if (!(carddata.max == 0 || carddata.max == -1))
            {
                if (carddata.max - tc[1] <= qrcode.Count)
                {
                    var ui = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
                    var done = false;
                    var isCancel = false;
                    ui.SetUI("{0}/{1} 卡片取得上限將超過當前人數，\n部分人將無法取得卡片是否繼續?",false,()=> { done = true; },()=> { done = true; isCancel = true; });
                    return Answer.PendingUntil(isCancel, () => { return done; });
                }
            }
            return Answer.Resolve(true);
        }).Then(result => {
            if ((bool)result)
            {
                UnityWebRequest www = HttpHelper.DoPost("card", new { playerqrcode = qrcode, cardid = task.cardid, from = task.qrcode });
                return Answer.Resolve(www);
            }
            else return Answer.Reject();     
        }).Then(result => {
            CleanUI();
            return Answer.Resolve();
        }).Reject(error => {
            if (string.IsNullOrEmpty(error) == false)
            {
                var ui = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
                ui.SetUI(error, true);
            }
            Debug.Log(error);
        }).Invoke(this);
    }

    bool CheckTaskCount(int tc)
    {
        if (task.max == -1 || task.max == 0)
        {
            return true;
        }
        else
        {
            return task.max >tc;
        }
        
    }
}

