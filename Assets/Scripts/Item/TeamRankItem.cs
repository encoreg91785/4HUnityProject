using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamRankItem : MonoBehaviour
{
    public TeamRank teamRank;
    public int rank;
    [SerializeField]
    Text teamNameText, amountText;
    string[] color = new string[5] { "999999", "FFACAC", "B1E5FF", "CDFFB1", "FFF1B1" };
    float[] player = new float[5] { 1f,12f, 12f, 12f, 11f  };
    public void SetTeamRank(TeamRank tr)
    {
        teamRank = tr;
        teamNameText.text = tr.team;
        
        var t = Utility.ParseEnum<TeamName>(tr.team);
        var total = t == TeamName.白虎? tr.amount - 1: tr.amount ;
        amountText.text = (total / player[(int)t]).ToString("f1");
        ColorUtility.TryParseHtmlString("#" + color[(int)t], out Color nowColor);
        GetComponent<Image>().color = nowColor;
    }

    public void Clear()
    {
        teamRank = null;
        amountText.text = teamNameText.text = "";
        ColorUtility.TryParseHtmlString("#444444", out Color nowColor);
        GetComponent<Image>().color = nowColor;
    }
}
