using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Linq;

public class RankUI : UIDialog
{
    [SerializeField]
    GameObject playerRankContent, teamRankContent, bulletinContent;

    int currentIndex =0 ;
    PlayerRank[] playerRankList = new PlayerRank[0];
    private void Start()
    {
       
    }

    void GetPlayerRankList()
    {
        new Promise().Then(_ => {
            UnityWebRequest www = HttpHelper.DoGet("rank/player");
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<PlayerRank[]>((string)result);
        }).Then(result => {
            playerRankList = (result as PlayerRank[]);
            SetPlayerRank();
            return Answer.Resolve();
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
    }

    void SetPlayerRank()
    {
        var ls = playerRankContent.GetComponentsInChildren<PlayerRankItem>();
        bool isEnd = false;
        for (int i = 0; i < 10; i++)
        {
            if (i + currentIndex < playerRankList.Length) ls[i].SetItem(playerRankList[i + currentIndex]);
            else
            {
                isEnd = true;
                ls[i].Clear();
            } 
        }
        if(isEnd) currentIndex=0;
        else currentIndex += 10;
    }

    void GetTeamRankList()
    {
        new Promise().Then(_ => {
            UnityWebRequest www = HttpHelper.DoGet("rank/team");
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<TeamRank[]>((string)result);
        }).Then(result => {
            var ls = (result as TeamRank[]);
            SetTeamRank(ls);
            return Answer.Resolve();
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
    }

    void SetTeamRank(TeamRank[] trls)
    {
        var ls = teamRankContent.GetComponentsInChildren<TeamRankItem>();
        for (int i = 0; i < trls.Length; i++)
        {
            var t = ls.ToList().Find(e => { return e.rank == trls[i].rank; });
            if (t != null) t.SetTeamRank(trls[i]);
        }
    }

    public void GetBulletinList()
    {
        Utility.LoadingPromise().Then(_ => {
            UnityWebRequest www = HttpHelper.DoGet("rank");
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<List<RankData>>((string)result);
        }).Then(result => {
            var ls = (result as Bulletin[]);
            SetBulletinItem(ls);
            return Answer.Resolve();
        }).Reject(error => {
            Debug.Log(error);
        }).Invoke(this);
    }

    public void SetBulletinItem(Bulletin[] buList)
    {
        var ls = bulletinContent.GetComponentsInChildren<Text>();
        for (int i = 0; i < ls.Length; i++)
        {
            if (buList.Length > i) ls[i].text = buList[i].message;
            else ls[i].text = "";
        }
    }

    public void Return()
    {
        UIManager.GetInstance().OpenDialog<QRCodeUI>("MainMenuUI");
        UIManager.GetInstance().CloseDialog(this);
    }
}
