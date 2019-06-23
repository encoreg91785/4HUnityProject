using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PasswordUI : UIDialog
{
    [SerializeField]
    InputField passwordInput;
    string password = "shenkeng";
    Action enterAciton = null;
    public void EnterAction(Action action)
    {
        enterAciton = action;
    }

    public void Enter()
    {
        if (passwordInput.text == password)
        {
            enterAciton?.Invoke();
            UIManager.GetInstance().CloseDialog(this);
        }
        else
        {
            passwordInput.text = "";
            var cf =UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
            cf.SetUI("密碼錯誤請詢問相關人員",true);
        }
    }

    public void Cancel()
    {
        UIManager.GetInstance().CloseDialog(this);
    }
}
