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

    public void SetTeamRank(TeamRank tr)
    {
        teamRank = tr;
        teamNameText.text = tr.team;
        amountText.text = tr.amount.ToString();
    }

    public void Clear()
    {
        teamRank = null;
        teamNameText.text = "";
        amountText.text = "";
    }
}
