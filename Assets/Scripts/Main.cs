using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    static Main instance = null;
    static public Main GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<Main>();
            if (instance == null) Debug.LogWarning("Main is not exist");
        }
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }
    [SerializeField]
    GameObject add,mainMenu,confirm,playerInformation,guildInformation,rank,rankMenu,receiveMission,missionMenu;
    private void Start()
    {
        mainMenu.gameObject.SetActive(true);
    }

    public void Add()
    {
        add.gameObject.SetActive(true);
    }

    public void Search()
    {
        playerInformation.gameObject.SetActive(true);
    }

    public void Rank()
    {
        rankMenu.gameObject.SetActive(true);
    }

    public void AllPlayer()
    {
        rank.gameObject.SetActive(true);
    }

    public void GuildRank()
    {
        guildInformation.gameObject.SetActive(true);
    }

    public void Confirm()
    {
        confirm.gameObject.SetActive(true);
    }

    public void Mission()
    {
        missionMenu.gameObject.SetActive(true);
    }

    public void ReceiveMission()
    {
        receiveMission.gameObject.SetActive(true);
    }
}
