using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : UIDialog
{
    public void ClickEdite()
    {
        var ps = UIManager.GetInstance().OpenDialog<PasswordUI>("PasswordUI");
        ps.EnterAction(()=> {
            UIManager.GetInstance().CloseDialog(this);
            UIManager.GetInstance().OpenDialog("EditeUI");
        });
    }

    public void ClickCard()
    {
        var ps = UIManager.GetInstance().OpenDialog<PasswordUI>("PasswordUI");
        ps.EnterAction(() => {
            UIManager.GetInstance().CloseDialog(this);
            UIManager.GetInstance().OpenDialog("ReceiveCardUI");
        });
    }

    public void ClickRank()
    {
        UIManager.GetInstance().CloseDialog(this);
        UIManager.GetInstance().OpenDialog("RankUI");
    }

    public void ClickTask()
    {
        UIManager.GetInstance().CloseDialog(this);
        UIManager.GetInstance().OpenDialog("ReceiveMissionUI");
    }

    public void ClickTransaction()
    {
        var ps = UIManager.GetInstance().OpenDialog<PasswordUI>("PasswordUI");
        ps.EnterAction(() => {
            UIManager.GetInstance().CloseDialog(this);
            UIManager.GetInstance().OpenDialog("TransactionCardUI");
        });
    }
}
