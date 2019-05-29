using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;

public class EditeUI : UIDialog
{
    [SerializeField]
    InputField rqcodeField, nameField;
    [SerializeField]
    Button enterBtn;
    Player playerData;
    Player newData;
    QRCodeUI ui;
    // Start is called before the first frame update
    void Start()
    {
        newData = new Player();
        enterBtn.interactable = false;
    }

    /// <summary>
    /// 修改名字
    /// </summary>
    /// <param name="str"></param>
    void OnNameFieldEndEdit(string str)
    {
        newData.name = nameField.text;
        CheckEnterEdite(newData.name != playerData.name);
    }

    /// <summary>
    /// 是否可以點擊確認修改
    /// </summary>
    /// <param name="isOn"></param>
    void CheckEnterEdite(bool isOn)
    {
        enterBtn.interactable = !(playerData == null) && isOn;
    }

    /// <summary>
    /// 執行修改
    /// </summary>
    public void EnterEdite()
    {
        UpdatePlayerData();
    }

    /// <summary>
    /// 返回主選單
    /// </summary>
    public void Return()
    {
        UIManager.GetInstance().OpenDialog<QRCodeUI>("MainMenuUI");
        UIManager.GetInstance().CloseDialog(this);
    }

    public void OpenQRCodeUI()
    {
        ui = UIManager.GetInstance().OpenDialog<QRCodeUI>("QRCodeUI");
        ui.resultAction = GetPlayerQRCode;
        ui.StartScan();
    }

    void UpdateUI(Player p)
    {
        rqcodeField.text = p.qrcode;
        nameField.text = p.name;
    }

    void UpdatePlayerData()
    {
        Utility.LoadingPromise().Then(_ => {
            UnityWebRequest www = HttpHelper.DoPut("player", new { playerqrcode = "player1",updateData= newData.ToJsonIngoreNull() });
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<Player>((string)result);
        }).Then(result => {
            playerData.UpdateData<Player>(newData);
            enterBtn.interactable = false;
            UpdateUI(playerData);
            return Answer.Resolve();
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
    }

    void GetPlayerQRCode(string qrcode)
    {
        if (string.IsNullOrEmpty(qrcode) == false)
        {
            GetPlayerData(qrcode);
            ui.StopScan();
        }
    }

    /// <summary>
    /// 從Server取得玩家資訊
    /// </summary>
    /// <param name="qrcode"></param>
    void GetPlayerData(string qrcode)
    {
        Utility.LoadingPromise().Then(_ => {
            UnityWebRequest www = HttpHelper.DoGet("player", new { playerqrcode = qrcode });
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<Player>((string)result);
        }).Then(result => {
            playerData = result as Player;
            UpdateUI(playerData);
            UIManager.GetInstance().CloseDialog("QRCodeUI");
            return Answer.Resolve();
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
    }

}
