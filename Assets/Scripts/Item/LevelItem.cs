using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class LevelItem : MonoBehaviour, IPointerDownHandler
{
    public bool active;
    public Action pointerDown;
    //[HideInInspector]
    public Level level;
    public GameObject frame;

    //public void OnDrag(PointerEventData eventData)
    //{
    //    if (!active) return;
    //    //Vector3 mousePos = eventData.position;
    //    //Vector3 targetPos = new Vector3(mousePos.x - Screen.width * 0.5f, mousePos.y - Screen.height * 0.5f, mousePos.z);
        
    //    //((RectTransform)transform).localPosition = mousePos;
    //    Vector2 localPosition = Vector2.zero;
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,Input.mousePosition,Camera.main,out localPosition);
    //    level.position = ((RectTransform)transform).TransformPoint(localPosition);
    //    ((RectTransform)transform).position = level.position;
    //}

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pointerDown == null) return;
        pointerDown();
    }

    public void SetFocus(bool focus)
    {
        frame.SetActive(focus);
    }
}