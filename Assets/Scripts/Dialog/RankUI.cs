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
        GetBulletinList();
        GetPlayerRankList();
        GetTeamRankList();
        var w = playerRankContent.GetComponent<RectTransform>().rect.width;
        var grid = playerRankContent.GetComponentInChildren<GridLayoutGroup>();
        grid.cellSize =new Vector2((w / 2.0f) - 24,grid.cellSize.y);
        InvokeRepeating("GetBulletinList",20,20);
        InvokeRepeating("GetTeamRankList", 20, 20);
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
            if (i + currentIndex < playerRankList.Length)
            {
                ls[i].SetItem(playerRankList[i + currentIndex]);
            } 
            else
            {
                isEnd = true;
                ls[i].Clear();
            } 
        }
        if (isEnd)
        {
            currentIndex = 0;
            Invoke(nameof(GetPlayerRankList), 5);
        }
        else
        {
            var time = currentIndex <= 10 ? 10 : 5;
            Invoke("SetPlayerRank", time);
            currentIndex += 10;
        } 
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
    float[] player = new float[5] { 1f,12f, 12f, 12f, 11f  };
    void SetTeamRank(TeamRank[] trls)
    {
        var ls = teamRankContent.GetComponentsInChildren<TeamRankItem>();
        var lss = trls.ToList();
        lss.Sort((a, b) => 
        {
            var at = Utility.ParseEnum<TeamName>(a.team);
            var bt = Utility.ParseEnum<TeamName>(b.team);
            var am = a.amount;
            var bm = b.amount;
            return am / player[(int)at] > bm / player[(int)bt] ? -1 : 1;
        });
        for (int i = 0; i < ls.Length; i++)
        {
            ls[i].SetTeamRank(lss[i]);
        }
    }

    public void GetBulletinList()
    {
        new Promise().Then(_ => {
            UnityWebRequest www = HttpHelper.DoGet("bulletin");
            return Answer.Resolve(www);
        }).Then(result => {
            return Utility.ParseServerRespond<Bulletin[]>((string)result);
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

    public void OnDestroy()
    {
        CancelInvoke();
    }
}
