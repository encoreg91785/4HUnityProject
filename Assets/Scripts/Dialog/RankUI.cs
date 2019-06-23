using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class RankUI : UIDialog
{
    [SerializeField]
    GameObject item,itemView;
    List<GameObject> itemList = new List<GameObject>();
    List<RankData> rankList = new List<RankData>();

    private void Start()
    {
        GetRankList();
    }

    void GetRankList()
    {
        Utility.LoadingPromise().Then(_ => {
            UnityWebRequest www = HttpHelper.DoGet("rank");
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<List<RankData>>((string)result);
        }).Then(result => {
            rankList = (result as List<RankData>);
            ShowRankItem();
            return Answer.Resolve();
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
    }

    public void Return()
    {
        UIManager.GetInstance().OpenDialog<QRCodeUI>("MainMenuUI");
        UIManager.GetInstance().CloseDialog(this);
    }

    void CleanUI()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            Destroy(itemList[i]);
        }
        itemList.Clear();
        rankList.Clear();
    }

    void ShowRankItem()
    {
        for (int i = 0; i < rankList.Count; i++)
        {
            var obj = Instantiate(item,itemView.transform);
            obj.SetActive(true);
            var ls = obj.GetComponentsInChildren<Text>();
            ls[0].text = (i+1).ToString();
            ls[1].text = rankList[i].playerqrcode;
            ls[2].text = rankList[i].num.ToString();
            itemList.Add(obj);
        }
    }
}

public class RankData
{
    public DateTime? lasttim;
    public string playerqrcode;
    public int num;
}
