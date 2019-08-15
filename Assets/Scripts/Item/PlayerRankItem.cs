using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRankItem : MonoBehaviour
{
    public PlayerRank pk;
    [SerializeField]
    Text nameText, rankText, teamText, amountText;

    public void SetItem(PlayerRank pk)
    {
        this.pk = pk;
        nameText.text = pk.name;
        rankText.text = pk.rank.ToString();
        teamText.text = pk.team;
        amountText.text = pk.amount.ToString();
        GetComponent<CanvasGroup>().alpha = 1;
    }

    public void Clear()
    {
        pk = null;
        nameText.text = "";
        rankText.text = "";
        teamText.text = ((TeamName)Convert.ToInt32(0)).ToString();
        amountText.text = "";
        GetComponent<CanvasGroup>().alpha = 0;
    }
}
