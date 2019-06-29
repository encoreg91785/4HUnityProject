using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class LevelItem : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public bool active;
    public Action pointerDown;
    //[HideInInspector]
    public Level level;
    public GameObject frame;

    public void OnDrag(PointerEventData eventData)
    {
        if (!active) return;
        Vector3 mousePos = Input.mousePosition;
        Vector3 targetPos = new Vector3(mousePos.x - Screen.width * 0.5f, mousePos.y - Screen.height * 0.5f, mousePos.z);
        level.position = targetPos;
        transform.localPosition = targetPos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!active && pointerDown == null) return;
        pointerDown();
    }

    public void SetFocus(bool focus)
    {
        frame.SetActive(focus);
    }
}