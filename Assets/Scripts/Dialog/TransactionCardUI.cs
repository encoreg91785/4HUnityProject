using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TransactionCardUI : UIDialog
{
    [SerializeField]
    InputField cardIdInput;
    [SerializeField]
    Text playerInfor,nameText, typeText, levelText;
    [SerializeField]
    Button enterBtn;
    CardData cardData;
    Player player;

    private void Start()
    {
        enterBtn.interactable = false;
    }

    public void ClearUIData()
    {
        player = null;
        playerInfor.text = "";
        nameText.text = "";
        typeText.text = "";
        levelText.text = "";
        cardIdInput.text = "";
        cardData = null;
        enterBtn.interactable = false;
    }

    public void Return()
    {
        UIManager.GetInstance().OpenDialog("MainMenuUI");
        UIManager.GetInstance().CloseDialog(this);
    }

    public void OpenQRCodeUI()
    {
        QRCodeUI ui = Main.GetInstance().OpenQRCodeUI();
        ui.OnShow();
        ui.resultAction = GetPlayerQRCode;
        ui.StartScan();
    }

    void GetPlayerQRCode(string qrcode)
    {
        GetPlayerData(qrcode);
        Main.GetInstance().CloseUIQRCode();
    }

    void GetPlayerData(string qrcode)
    {
        Utility.LoadingPromise().Then(_ => {
            UnityWebRequest www = HttpHelper.DoGet("player", new { playerqrcode = qrcode });
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<Player>((string)result);
        }).Then(result => {
            player = result as Player;
            playerInfor.text = string.Format("名稱:{0}\nQRCode:{1}", player.name, player.qrcode);
            return Answer.Resolve();
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
    }

    void GetCard()
    {
        Utility.LoadingPromise().Then(_ => {
            UnityWebRequest www = HttpHelper.DoPost("card", new { playerqrcode = new List<string>() { player.qrcode }, cardid = cardData.id, from = "transaction", confirm = DateTime.Now.Ticks });
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<string>((string)result);
        }).Then(result => {
            var u = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
            u.SetUI("成功", true);
            ClearUIData();
            return Answer.Resolve();
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
    }

    public void OnConfirmTransactionCard()
    {
        var ui = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
        ui.SetUI("確定執行", false, () => { GetCard(); });
    }

    /// <summary>
    /// 查詢卡片資訊
    /// </summary>
    public void OnClickSearchCard()
    {
        var cardId = cardIdInput.text;
        string msg = null;
        if (string.IsNullOrWhiteSpace(cardId) == false)
        {
            var card = Main.GetInstance().GetCardData(cardId);
            if (card != null)
            {
                nameText.text = card.name;
                typeText.text = card.tpye;
                levelText.text = card.level;
                cardData = card;
                enterBtn.interactable = player != null;
            }
            else msg = cardId + " : 卡片不存在";
        }
        if (msg != null)
        {
            var ui = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
            ui.SetUI(msg, true);
        }
    }
}
