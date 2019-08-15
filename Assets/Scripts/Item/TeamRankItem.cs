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
    string[] color = new string[5] { "444444", "FFACAC", "B1E5FF", "CDFFB1", "FFF1B1" };
    public void SetTeamRank(TeamRank tr)
    {
        teamRank = tr;
        teamNameText.text = tr.team;
        amountText.text = tr.amount.ToString();
        var t = Utility.ParseEnum<TeamName>(tr.team);
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
