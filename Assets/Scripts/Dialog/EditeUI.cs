using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EditeUI : UIDialog
{
    [SerializeField]
    InputField rqcodeField, nameField, teamField;
    [SerializeField]
    Button enterBtn;
    Player playerData;
    Player newData;
    // Start is called before the first frame update
    void Start()
    {
        nameField.onEndEdit.AddListener(OnNameFieldEndEdit);
        teamField.onEndEdit.AddListener(OnTeamFieldEndEdit);
        ClearUI();
    }

    /// <summary>
    /// 修改名字
    /// </summary>
    /// <param name="str"></param>
    void OnNameFieldEndEdit(string str)
    {
        if (playerData == null) return;
        newData.name = nameField.text;
        CheckEnterEdite(newData.name != playerData.name);
    }

    // <summary>
    /// 修改隊伍
    /// </summary>
    /// <param name="str"></param>
    void OnTeamFieldEndEdit(string str)
    {
        if (playerData == null) return;
        newData.team = teamField.text;
        CheckEnterEdite(newData.team != playerData.team);
    }

    /// <summary>
    /// 是否可以點擊確認修改
    /// </summary>
    /// <param name="isOn"></param>
    void CheckEnterEdite(bool isOn)
    {
        enterBtn.interactable = !(playerData == null) && isOn;
    }

    /// <summary>
    /// 執行修改
    /// </summary>
    public void EnterEdite()
    {
        var ui = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
        ui.SetUI("確定執行", false, () => { UpdatePlayerData(); });
    }

    /// <summary>
    /// 返回主選單
    /// </summary>
    public void Return()
    {
        UIManager.GetInstance().OpenDialog("MainMenuUI");
        UIManager.GetInstance().CloseDialog(this);
    }

    public void OpenQRCodeUI()
    {
        QRCodeUI ui = Main.GetInstance().OpenQRCodeUI();
        ui.resultAction = GetPlayerQRCode;
        ui.StartScan();
    }

    void UpdateUI(Player p)
    {
        rqcodeField.text = p.qrcode;
        nameField.text = p.name;
        teamField.text = p.team;
    }

    void UpdatePlayerData()
    {
        Utility.LoadingPromise().Then(_ =>
        {
            UnityWebRequest www = HttpHelper.DoPut("player", new { playerqrcode = playerData.qrcode, updateData = newData.ToJsonIngoreNull() });
            return Answer.Resolve(www);
        }).Then(result =>
        {
            return Utility.ParseServerRespond<Player>((string)result);
        }).Then(result =>
        {
            playerData.UpdateData<Player>(newData);
            enterBtn.interactable = false;
            UpdateUI(playerData);
            var u = UIManager.GetInstance().OpenDialog<ConfirmUI>("ConfirmUI");
            u.SetUI("成功", true);
            return Answer.Resolve();
        }).Reject(error =>
        {
            Debug.Log(error);
        }).Invoke(this);
    }

    void GetPlayerQRCode(string qrcode)
    {
        GetPlayerData(qrcode);
        Main.GetInstance().CloseUIQRCode();
    }

    /// <summary>
    /// 從Server取得玩家資訊
    /// </summary>
    /// <param name="qrcode"></param>
    void GetPlayerData(string qrcode)
    {
        Utility.LoadingPromise().Then(_ =>
        {
            UnityWebRequest www = HttpHelper.DoGet("player", new { playerqrcode = qrcode });
            return Answer.Resolve(www);
        }).Then(result =>
        {
            return Utility.ParseServerRespond<Player>((string)result);
        }).Then(result =>
        {
            playerData = result as Player;
            UpdateUI(playerData);
            return Answer.Resolve();
        }).Reject(error =>
        {
            Debug.Log(error);
        }).Invoke(this);
    }

    public void ClearUI()
    {
        newData = new Player();
        enterBtn.interactable = false;
        playerData = null;
        rqcodeField.text = "";
        nameField.text = "";
        teamField.text = "";
    }
}
