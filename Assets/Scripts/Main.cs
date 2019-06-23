using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

public class Main : MonoBehaviour
{
    static Main instance = null;
    Dictionary<string, CardData> cardDatas = new Dictionary<string, CardData>();
    Dictionary<string, TaskData> taskDatas = new Dictionary<string, TaskData>();
    QRCodeUI uiQRCode;
    static public Main GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<Main>();
            if (instance == null) Debug.LogWarning("Main is not exist");
        }
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 30;
        uiQRCode = UIManager.GetInstance().OpenDialog<QRCodeUI>("QRCodeUI");
        uiQRCode.OnHide();
        Init().Then(_ =>
        {
            return Answer.Resolve();
        }).Then(_ =>
        {
            UIManager.GetInstance().OpenDialog<MainMenuUI>("MainMenuUI");
            return Answer.Resolve();
        }).Invoke(this);
    }

    public void CloseUIQRCode()
    {
        if (uiQRCode == null) Debug.LogError("QRCodeUI is Null");
        uiQRCode.OnHide();
    }

    public QRCodeUI OpenQRCodeUI()
    {
        if (uiQRCode == null) Debug.LogError("QRCodeUI is Null");
        uiQRCode.OnShow();
        return uiQRCode;
    }

    public CardData GetCardData(string id)
    {
        CardData c = null;
        cardDatas.TryGetValue(id, out c);
        return c;
    }

    public TaskData GetTaskData(string qrcode)
    {
        TaskData t = null;
        taskDatas.TryGetValue(qrcode, out t);
        return t;
    }

    Promise Init()
    {
         return GetServerIp().Then(_ => {
             UnityWebRequest www = HttpHelper.DoGet("temp");
             return Answer.Resolve(www);
         }).Then(result => {
             return Utility.ParseServerRespond<Dictionary<string,Dictionary<string,object>>>((string)result);
         }).Then(result => {
             Dictionary<string, Dictionary<string, object>> datas = (Dictionary<string, Dictionary<string, object>>)result;
             foreach (var item in datas["card"])
             {
                 CardData c = JsonConvert.DeserializeObject<CardData>(item.Value.ToString());
                 cardDatas.Add(item.Key, c);
             }
             foreach (var item in datas["task"])
             {
                 TaskData t = JsonConvert.DeserializeObject<TaskData>(item.Value.ToString());
                 taskDatas.Add(item.Key, t);
             }
             return Answer.Resolve();
         });
        
    }

    /// <summary>
    /// 取得ServerIP
    /// 產生Promise但是尚未執行
    /// </summary>
    /// <returns></returns>
    Promise GetServerIp()
    {
        LoadingUI load = UIManager.GetInstance().OpenDialog<LoadingUI>("LoadingUI");
        return new Promise().Done(() =>
        {
            UIManager.GetInstance().CloseDialog("LoadingUI");
        });
        return new Promise().Then(_ =>
        {
            UnityWebRequest www = HttpHelper.DoGet("https://rpg4hproject.firebaseio.com/IP.json", null, false);
            return Answer.Resolve(www);
        }).Then(ip => {
            var d = JsonConvert.DeserializeObject<Dictionary<string, string>>((string)ip);
            HttpHelper.SetHostAndPort(d["ip"], 12121);
            return Answer.Resolve();
        }).Done(()=>{
            UIManager.GetInstance().CloseDialog("LoadingUI");
        }).Reject(error=> {
            Debug.Log(error);
        });
    }
}


