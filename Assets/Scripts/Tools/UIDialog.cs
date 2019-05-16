using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup))]
public class UIDialog : MonoBehaviour
{
#region 你可以自由繼承的函式
    public virtual void OnShow()
    {
        GetComponent<CanvasGroup>().Active(true);
    }

    public virtual void OnHide(bool doOnClose = false)
    {
        GetComponent<CanvasGroup>().Active(false);
    }

    public virtual void OnLocaleChange()
    {

    }
    #endregion
    /// <summary>
    /// 加一層全螢幕感應區域在最底層，以屏蔽下層的Dialog訊息
    /// </summary>
    /// <param name="color"></param>
    public void AttachFullscreenMask(Color color)
    {
        GameObject panel = new GameObject("Panel");
        panel.AddComponent<CanvasRenderer>();
        Image i = panel.AddComponent<Image>();
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        i.color = color;
        panel.transform.SetParent(transform, false);
        panel.transform.SetAsFirstSibling();
    }
}
