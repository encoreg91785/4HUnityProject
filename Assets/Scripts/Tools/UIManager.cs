using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIManager
/// </summary>
public class UIManager : MonoBehaviour
{
    static UIManager instance = null;
    Dictionary<string, object> dialogPrefabs = new Dictionary<string, object>();
    List<UIDialog> dialogInstances = new List<UIDialog>();
    Canvas clearDepthCanvas;
    string path = "UIDialogs/";
    static public UIManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<UIManager>();
            if (instance == null) Debug.LogWarning("UIManager is not exist");
        }
        return instance;
    }

    private void Awake()
    {
        instance = this;
        clearDepthCanvas = GetComponent<Canvas>();
    }

    public UIDialog OpenDialog(string name)
    {
        var dlg = LoadDialog(name);
        dlg.transform.SetParent(clearDepthCanvas.transform, false);
        return dlg;
    }

    public T OpenDialog<T>(string name) where T: UIDialog
    {
        var ui = OpenDialog(name);
        return ui==null?null: ui as T;
    }

    public UIDialog GetDialog(string name)
    {
        for (int i = 0; i < dialogInstances.Count; i++)
        {
            var dlg = dialogInstances[i];
            if (dlg.gameObject.name == name) return dlg;
        }
        Debug.Log("GetDialog:" + name + "is not instances");
        return null;
    }

    public void CloseDialog(UIDialog dlg)
    {
        if (dlg != null)
        {
            dialogInstances.Remove(dlg);
            Destroy(dlg.gameObject);
        }
        else
        {
            Debug.LogWarning("Dialog Is null");
        }
    }

    public void CloseDialog(string name)
    {
        var dlg = GetDialog(name);
        if (dlg != null) CloseDialog(dlg);
    }

    public void BringToTop(UIDialog dlg)
    {
        var tran = clearDepthCanvas.transform;
        if (dlg.transform.parent.gameObject != clearDepthCanvas.gameObject) Debug.LogWarning(dlg.gameObject.name + "is in clearDepthCanvas");
        else
        {
            dlg.transform.SetAsLastSibling();
        }
    }

    public UIDialog GetTopDialog()
    {
        var go = clearDepthCanvas.transform.GetChild(0);
        var dlg = go.GetComponent<UIDialog>();
        return dlg;
    }

    /// <summary>
    /// LoadUIDialog
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    UIDialog LoadDialog(string name)
    {
        if (dialogPrefabs.ContainsKey(name) == false)
        {
            var obj = Resources.Load<GameObject>(path + name);
            if (obj == null) Debug.LogWarning("NotExist:" + name);
            dialogPrefabs[name] = obj;
        }
        var uiObj = Instantiate((GameObject)dialogPrefabs[name]);
        var dlg = uiObj.GetComponent<UIDialog>();
        uiObj.name = name;
        if (dlg == null)
        {
            Destroy(uiObj);
            Debug.Log(name + "Dont Have UIDialog Script");
            return null;
        }

        //設置Dialog
        uiObj.name = name;
        RectTransform prefabRect = ((GameObject)dialogPrefabs[name]).transform as RectTransform;
        RectTransform newRectTransform = uiObj.transform as RectTransform;
        newRectTransform.anchoredPosition = prefabRect.anchoredPosition;
        newRectTransform.anchorMax = prefabRect.anchorMax;
        newRectTransform.anchorMin = prefabRect.anchorMin;
        newRectTransform.localRotation = prefabRect.localRotation;
        newRectTransform.localScale = prefabRect.localScale;
        newRectTransform.pivot = prefabRect.pivot;
        newRectTransform.sizeDelta = prefabRect.sizeDelta;

        dialogInstances.Add(dlg);
        return dlg;
    }
}
