using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ReceiveCardUI : UIDialog
{
    [SerializeField]
    CardItem cardListItme;
    List<Card> cardList = new List<Card>();
    Player playerData;
    [SerializeField]
    Text showText;
    [SerializeField]
    ScrollRect listView;
    [SerializeField]
    Button confirmBtn;
    List<CardItem> viewItem =new List<CardItem>();

    private void Start()
    {
        confirmBtn.interactable = false;
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

    void SetTaskObjectToScrollView()
    {
        if (cardList != null && cardList.Count > 0)
        {
            for (int i = 0; i < cardList.Count; i++)
            {
                var obj =Instantiate(cardListItme, listView.content);
                obj.gameObject.SetActive(true);
                viewItem.Add(obj);
                obj.SetCard(cardList[i]);
            }
        }
    }

    public void ClearUIData()
    {
        playerData = null;
        showText.text = "";
        viewItem.ForEach(o => { Destroy(o.gameObject); });
        viewItem.Clear();
        cardList.Clear();
        confirmBtn.interactable = false;
    }

    public void ConfirmTask()
    {
        Utility.LoadingPromise().Then(_ => {
            List<int> idList = new List<int>();
            for (int i = 0; i < viewItem.Count; i++)
            {
                if(viewItem[i].ReceiveNow()) idList.Add(viewItem[i].card.id);
            }
            UnityWebRequest www = HttpHelper.DoPut("card", new { cardid = idList });
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<object>((string)result);
        }).Then(_ => {
            ClearUIData();
            return Answer.Resolve();
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
        
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
            playerData = result as Player;
            showText.text = string.Format("QRCode:{0}\n名稱:{1}\n隊伍:{2}",playerData.qrcode ,playerData.name,playerData.team);
            UnityWebRequest www = HttpHelper.DoGet("card", new { playerqrcode = qrcode });
            return Answer.Resolve(www);
        }).Then(result => {
            
            return Utility.ParseServerRespond<List<Card>>((string)result);
        }).Then(result =>
        {
            if (result != null) {
                cardList = result as List<Card>;
                confirmBtn.interactable = cardList != null && cardList.Count > 0;
                SetTaskObjectToScrollView();
            }
            return Answer.Resolve();
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
    }
}
