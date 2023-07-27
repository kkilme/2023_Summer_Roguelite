using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour, IPointerDownHandler
{
    public Image image;
    public TextMeshProUGUI text;
    public Action<ItemUI> action;

    public void OnPointerDown(PointerEventData eventData)
    {
        action?.Invoke(this);
    }
}
