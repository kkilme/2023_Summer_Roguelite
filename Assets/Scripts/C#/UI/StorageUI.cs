using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StorageUI : InventoryUI
{
    private void Awake()
    {
        inventory = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Inventory>();
        rectTransform = transform.GetChild(0).GetComponent<RectTransform>();

        inventoryItemUIStack = new Stack<ItemUI>(transform.GetChild(0).GetComponentsInChildren<ItemUI>(true));
        scrollRect = GetComponentInChildren<ScrollRect>(true);
        inventoryTile = transform.GetChild(0).gameObject;

        width = tileSizeWidth * transform.parent.GetComponent<RectTransform>().localScale.x;
        height = tileSizeWidth * transform.parent.GetComponent<RectTransform>().localScale.y;
    }

    private void OnEnable()
    {
        inventory.OnInventoryChanged += DisplayInventoryUI;
    }

    private void OnDisable()
    {
        inventory.OnInventoryChanged -= DisplayInventoryUI;
    }
}
