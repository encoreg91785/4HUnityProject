using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

public class Main : MonoBehaviour
{
    static Main instance = null;
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
        //LoadingUI load = UIManager.GetInstance().OpenDialog<LoadingUI>("LoadingUI");

        //GetServerIp().Then(_ =>
        //{
        //    GetPlayerData();
        //    return Answer.Resolve(new WaitForSeconds(1));
        //}).Then(_ =>
        //{
        //    UIManager.GetInstance().CloseDialog(load);
        //    UIManager.GetInstance().OpenDialog<MainMenuUI>("MainMenuUI");
        //    return Answer.Resolve();
        //}).Invoke(this);
    }

    /// <summary>
    /// 取得ServerIP
    /// 產生Promise但是尚未執行
    /// </summary>
    /// <returns></returns>
    Promise GetServerIp()
    {
        return new Promise();
        return new Promise().Then(_ =>
        {
            UnityWebRequest www = HttpHelper.DoGet("https://rpg4hproject.firebaseio.com/IP.json", null, false);
            return Answer.Resolve(www);
        }).Then(ip => {
            Debug.Log(((UnityWebRequest)ip).downloadHandler.text);
            var d = JsonConvert.DeserializeObject<Dictionary<string, string>>(((UnityWebRequest)ip).downloadHandler.text);
            HttpHelper.SetHostAndPort(d["ip"], 12121);
            return Answer.Resolve();
        });
    }
}


