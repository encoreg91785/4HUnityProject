using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmUI : UIDialog
{
    [SerializeField]
    Button enterBtn, cancelBtn;
    [SerializeField]
    Text msg;
    Action enter, cancel;
    public void SetUI(string msg,bool isOneBtn,Action enter=null, Action cancel=null)
    {
        if(string.IsNullOrEmpty(msg)==false)this.msg.text = msg;
        this.enter = enter;
        this.cancel = cancel;
        cancelBtn.gameObject.SetActive(!isOneBtn);
    }

    public void OnClickEnter()
    {
        enter?.Invoke();
        UIManager.GetInstance().CloseDialog(this);
    }

    public void OnClickCancel()
    {
        cancel?.Invoke();
        UIManager.GetInstance().CloseDialog(this);
    }
}
