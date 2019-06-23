using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    [SerializeField]
    Text nameText, teamText;
    [SerializeField]
    Toggle isSelect;
    public Player player;

    public void SetPalyer(Player p)
    {
        player = p;
        nameText.text = p.name;
        teamText.text = p.team;
        isSelect.isOn = true;
    }

    public bool IsSelect()
    {
        return isSelect.isOn;
    }
}
