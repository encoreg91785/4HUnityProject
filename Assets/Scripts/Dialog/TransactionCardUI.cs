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
        nameText.text = "";
        typeText.text = "";
        levelText.text = "";
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
        UIManager.GetInstance().GetDialog("QRCodeUI").OnHide();
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

    public void OnConfirmTransactionCard()
    {
    }

    /// <summary>
    /// 查詢卡片資訊
    /// </summary>
    public void OnCliclSearchCard()
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
        }
        if (msg != null)
        {
            var ui = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
            ui.SetUI(msg, true);
        }
    }
}
