using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine;
using UnityEngine.UI;

public class StorageUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private const int tileSizeWidth = 64;
    private const int tileSizeHeight = 64;
    private float width;
    private float height;
    private Storage storage;

    private ScrollRect scrollRect;
    private GameObject inventoryTile;

    private ItemUI selectedStorageItem;
    public PlayersInventoryItem selectedInventoryItem;

    private Dictionary<PlayersInventoryItem, ItemUI> inventoryDic = new Dictionary<PlayersInventoryItem, ItemUI>();
    private Dictionary<PlayersInventoryItem, ItemUI> storageDic = new Dictionary<PlayersInventoryItem, ItemUI>();

    private Stack<ItemUI> inventoryItemUIStack;
    private Stack<ItemUI> storageItemUIStack;

    [SerializeField] private TextMeshProUGUI scrapText;

    private void Awake()
    {
        storage = GetComponent<Storage>();
        inventoryItemUIStack = new Stack<ItemUI>(transform.GetChild(0).GetComponentsInChildren<ItemUI>(true));
        storageItemUIStack = new Stack<ItemUI>(transform.GetChild(1).GetComponentsInChildren<ItemUI>(true));
        rectTransform = transform.GetChild(0).GetComponent<RectTransform>();
        storageItemUIStack.ToList().ForEach(x => x.action += SelectStorageItem);
        scrollRect = GetComponentInChildren<ScrollRect>(true);
        inventoryTile = transform.GetChild(0).gameObject;

        width = tileSizeWidth * transform.parent.GetComponent<RectTransform>().localScale.x;
        height = tileSizeWidth * transform.parent.GetComponent<RectTransform>().localScale.y;
    }

    public void Init(List<PlayersInventoryItem> storageItems)
    {
        var removedItems = storageDic.Keys.Except(storageItems).ToArray();

        for (int i = 0; i < removedItems.Length; i++)
        {
            storageItemUIStack.Push(storageDic[removedItems[i]]);
            storageDic[removedItems[i]].gameObject.SetActive(false);
            storageDic.Remove(removedItems[i]);
        }

        for (int i = 0; i < storageItems.Count; i++)
        {
            if (!storageDic.ContainsKey(storageItems[i]))
            {
                storageDic.Add(storageItems[i], storageItemUIStack.Pop());
            }

            storageDic[storageItems[i]].gameObject.SetActive(true);

            var data = storageItems[i].InstanceData.GetAs<Storage.StorageItemData>();
            storageDic[storageItems[i]].image.rectTransform.sizeDelta = new Vector2(data.sizeX, data.sizeY) * 64;
            storageDic[storageItems[i]].text.text = data.currentCount.ToString();
        }
    }

    private async void OnEnable()
    {
        storage.OnStorageChanged += DisplayStorageItem;
        storage.OnInventoryChanged += DisplayInventoryUI;
        var balances = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
        int amount = (int)balances.Balances[0].Balance;
        scrapText.text = $"Scrap: {amount}";
    }

    private void OnDisable()
    {
        storage.OnStorageChanged -= DisplayStorageItem;
        storage.OnInventoryChanged -= DisplayInventoryUI;
    }

    private void Update()
    {
        if (selectedInventoryItem != null)
        {
            var pos = GetGridPostion(Input.mousePosition);
            inventoryDic[selectedInventoryItem].image.rectTransform.localPosition = new Vector2(pos.x, pos.y) * 64;
        }
        if (selectedStorageItem != null)
        {
            var pos = GetGridPostion(Input.mousePosition);
            selectedStorageItem.image.rectTransform.localPosition = new Vector2(pos.x, pos.y) * 64;
        }
        if (Input.GetMouseButtonDown(0))
        {
            var pos = GetGridPostion(Input.mousePosition);
            selectedInventoryItem = storage.SelectItem(pos.x, pos.y);
        }
        if (Input.GetMouseButtonUp(0))
        {
            Vector2Int pos = GetGridPostion(Input.mousePosition);
            DropItem(pos);
            MoveItem(pos);
            PutItem(pos);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }
    }

    public void DisplayInventoryUI(object sender, Storage.InventoryEventHandlerArgs e)
    {
        // 인벤토리에서 제거된 아이템 추출 및 삭제
        List<PlayersInventoryItem> inventoryItems = new List<PlayersInventoryItem>();

        for (int i = 0; i < e.InventoryItems.Count; i++)
            inventoryItems.Add(e.InventoryItems[i]);

        var removedItems = inventoryDic.Keys.Except(inventoryItems).ToArray();

        for (int i = 0; i < removedItems.Length; i++)
        {
            inventoryItemUIStack.Push(inventoryDic[removedItems[i]]);
            inventoryDic[removedItems[i]].gameObject.SetActive(false);
            inventoryDic.Remove(removedItems[i]);
        }

        for (int i = 0; i < e.InventoryItems.Count; i++)
        {
            if (!inventoryDic.ContainsKey(e.InventoryItems[i]))
            {
                inventoryDic.Add(e.InventoryItems[i], inventoryItemUIStack.Pop());
            }

            var data = e.InventoryItems[i].InstanceData.GetAs<Storage.StorageItemData>();

            inventoryDic[e.InventoryItems[i]].gameObject.SetActive(true);
            //itemImages[i] = e.Items[i].ItemStat.image;
            //itemImages[i].SetNativeSize();

            if (!data.isRight)
                inventoryDic[e.InventoryItems[i]].image.rectTransform.sizeDelta = new Vector2(data.sizeY, data.sizeX) * 64;
            else
                inventoryDic[e.InventoryItems[i]].image.rectTransform.sizeDelta = new Vector2(data.sizeX, data.sizeY) * 64;

            inventoryDic[e.InventoryItems[i]].text.text = data.currentCount.ToString();

            if (selectedInventoryItem != null)
            {
                if (!e.InventoryItems[i].Equals(selectedInventoryItem))
                    inventoryDic[e.InventoryItems[i]].image.rectTransform.localPosition = new Vector2(data.posX, data.posY) * 64;
            }
            else
                inventoryDic[e.InventoryItems[i]].image.rectTransform.localPosition = new Vector2(data.posX, data.posY) * 64;
        }
    }

    public void DisplayStorageItem(object sender, Storage.StorageItemEventHandlerArgs e)
    {
        if (e.changedType == Storage.StorageItemEventHandlerArgs.ChangedType.Added)
        {
            if (!storageDic.ContainsKey(e.item))
            {
                storageDic.Add(e.item, storageItemUIStack.Pop());
            }

            storageDic[e.item].gameObject.SetActive(true);
            var data = e.item.InstanceData.GetAs<Storage.StorageItemData>();
            storageDic[e.item].image.rectTransform.sizeDelta = new Vector2(data.sizeX, data.sizeY) * 64;
            storageDic[e.item].text.text = data.currentCount.ToString();
        }

        else
        {
            if (storageDic.ContainsKey(e.item))
            {
                storageItemUIStack.Push(storageDic[e.item]);
                storageDic[e.item].gameObject.SetActive(false);
                storageDic.Remove(e.item);
            }
        }
    }

    private void SelectStorageItem(ItemUI itemUI)
    {
        selectedStorageItem = itemUI;
        selectedStorageItem.transform.SetParent(inventoryTile.transform);
    }

    private Vector2Int GetGridPostion(Vector2 mousePosition)
    {
        Vector2Int gridPos = Vector2Int.zero;

        gridPos.x = Mathf.FloorToInt((mousePosition.x - rectTransform.position.x) / width);
        gridPos.y = Mathf.FloorToInt((mousePosition.y - rectTransform.position.y) / height);

        return gridPos;
    }

    private void MoveItem(Vector2Int pos)
    {
        if (selectedInventoryItem != null)
        {
            var t = selectedInventoryItem;
            selectedInventoryItem = null;
            storage.MoveItem(t, pos.x, pos.y);
        }
    }

    private void RotateItem()
    {
        if (selectedInventoryItem != null)
        {
            storage.RotateItem(selectedInventoryItem);
        }
    }

    private void PutItem(Vector2Int pos)
    {
        if (selectedStorageItem != null)
        {
            var item = storageDic.ToList().Find(x => x.Value == selectedStorageItem).Key;
            storage.PutItem(item, pos.x, pos.y);
            selectedStorageItem.transform.SetParent(scrollRect.transform.GetChild(0).GetChild(0));
            selectedStorageItem = null;
        }
    }

    private void DropItem(Vector2Int pos)
    {
        if (selectedInventoryItem != null && (pos.x < 0 || pos.y < 0 || pos.x >= storage.sizeX || pos.y >= storage.sizeY))
        {
            storage.RemoveItem(selectedInventoryItem);
            selectedInventoryItem = null;
        }
    }
}
