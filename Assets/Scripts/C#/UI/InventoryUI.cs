using Cysharp.Threading.Tasks.Triggers;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private const int tileSizeWidth = 64;
    private const int tileSizeHeight = 64;
    private float width;
    private float height;
    private Inventory inventory;

    private ScrollRect scrollRect;
    private GameObject inventoryTile;

    public InventoryItem selectedInventoryItem;
    private ItemUI selectedNearItem;

    private Stack<ItemUI> inventoryItemUIStack;
    private Stack<ItemUI> nearItemUIStack;

    private Dictionary<InventoryItem, ItemUI> inventoryDic = new Dictionary<InventoryItem, ItemUI>();
    private Dictionary<GettableItem, ItemUI> nearDic = new Dictionary<GettableItem, ItemUI>();

    private void Awake()
    {
        inventory = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Inventory>();
        rectTransform = transform.GetChild(0).GetComponent<RectTransform>();

        inventoryItemUIStack = new Stack<ItemUI>(transform.GetChild(0).GetComponentsInChildren<ItemUI>(true));
        nearItemUIStack = new Stack<ItemUI>(transform.GetChild(1).GetComponentsInChildren<ItemUI>(true));
        nearItemUIStack.ToList().ForEach(x => x.action += SelectNearItem);
        scrollRect = GetComponentInChildren<ScrollRect>(true);
        inventoryTile = transform.GetChild(0).gameObject;

        width = tileSizeWidth * transform.parent.GetComponent<RectTransform>().localScale.x;
        height = tileSizeWidth * transform.parent.GetComponent<RectTransform>().localScale.y;
    }

    private void OnEnable()
    {
        DisplayNearItemUI();
        inventory.OnInventoryChanged += DisplayInventoryUI;
        inventory.OnNearItemChanged += DisplayNearItemUI;
        inventory.EnableInventoryUI();
    }

    private void OnDisable()
    {
        inventory.OnInventoryChanged -= DisplayInventoryUI;
        inventory.OnNearItemChanged -= DisplayNearItemUI;
    }

    private void Update()
    {
        if (selectedInventoryItem.itemName != ITEMNAME.NONE)
        {
            var pos = GetGridPostion(Input.mousePosition);
            inventoryDic[selectedInventoryItem].image.rectTransform.localPosition = new Vector2(pos.x, pos.y) * 64;
        }
        if (selectedNearItem != null)
        {
            var pos = GetGridPostion(Input.mousePosition);
            selectedNearItem.image.rectTransform.localPosition = new Vector2(pos.x, pos.y) * 64;
        }
        if (Input.GetMouseButtonDown(0))
        {
            var pos = GetGridPostion(Input.mousePosition);
            selectedInventoryItem = inventory.SelectItem(pos.x, pos.y);
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

    private void DisplayInventoryUI(Inventory.InventoryEventHandlerArgs e)
    {
        // 인벤토리에서 제거된 아이템 추출 및 삭제
        List<InventoryItem> inventoryItems = new List<InventoryItem>();

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

            inventoryDic[e.InventoryItems[i]].gameObject.SetActive(true);
            //itemImages[i] = e.Items[i].ItemStat.image;
            //itemImages[i].SetNativeSize();

            if (e.InventoryItems[i].rotationType.Equals(ROTATION_TYPE.TOP))
                inventoryDic[e.InventoryItems[i]].image.rectTransform.sizeDelta = new Vector2(e.InventoryItems[i].sizeY, e.InventoryItems[i].sizeX) * 64;
            else
                inventoryDic[e.InventoryItems[i]].image.rectTransform.sizeDelta = new Vector2(e.InventoryItems[i].sizeX, e.InventoryItems[i].sizeY) * 64;

            inventoryDic[e.InventoryItems[i]].text.text = e.InventoryItems[i].currentCount.ToString();

            if (selectedInventoryItem.itemName != ITEMNAME.NONE)
            {
                if (!e.InventoryItems[i].Equals(selectedInventoryItem))
                    inventoryDic[e.InventoryItems[i]].image.rectTransform.localPosition = new Vector2(e.InventoryItems[i].posX, e.InventoryItems[i].posY) * 64;
            }
            else
                inventoryDic[e.InventoryItems[i]].image.rectTransform.localPosition = new Vector2(e.InventoryItems[i].posX, e.InventoryItems[i].posY) * 64;
        }
    }

    private void DisplayNearItemUI(object sender, Inventory.NearItemEventHandlerArgs e)
    {
        if (e.changedType == Inventory.NearItemEventHandlerArgs.ChangedType.Added)
        {
            if (!nearDic.ContainsKey(e.GettableItem))
            {
                nearDic.Add(e.GettableItem, nearItemUIStack.Pop());
            }

            nearDic[e.GettableItem].gameObject.SetActive(true);
            var stat = Item.GetItemStat(e.GettableItem.ItemName, e.GettableItem.ItemCount);
            nearDic[e.GettableItem].image.rectTransform.sizeDelta = new Vector2(stat.sizeX, stat.sizeY) * 64;
            nearDic[e.GettableItem].text.text = stat.currentCount.ToString();
        }

        else
        {
            if (nearDic.ContainsKey(e.GettableItem))
            {
                nearItemUIStack.Push(nearDic[e.GettableItem]);
                nearDic[e.GettableItem].gameObject.SetActive(false);
                nearDic.Remove(e.GettableItem);
            }
        }
    }

    private void DisplayNearItemUI()
    {
        var nearItems = inventory.GetNearItems();
        var removedItems = nearDic.Keys.Except(nearItems).ToArray();

        for (int i = 0; i < removedItems.Length; i++)
        {
            nearItemUIStack.Push(nearDic[removedItems[i]]);
            nearDic[removedItems[i]].gameObject.SetActive(false);
            nearDic.Remove(removedItems[i]);
        }

        for (int i = 0; i < nearItems.Count; i++)
        {
            if (!nearDic.ContainsKey(nearItems[i]))
            {
                nearDic.Add(nearItems[i], nearItemUIStack.Pop());
            }

            nearDic[nearItems[i]].gameObject.SetActive(true);
            var stat = Item.GetItemStat(nearItems[i].ItemName, nearItems[i].ItemCount);
            nearDic[nearItems[i]].image.rectTransform.sizeDelta = new Vector2(stat.sizeX, stat.sizeY) * 64;
            nearDic[nearItems[i]].text.text = stat.currentCount.ToString();
        }
    }

    private void MoveItem(Vector2Int pos)
    {
        if (selectedInventoryItem.itemName != ITEMNAME.NONE)
        {
            var t = selectedInventoryItem;
            selectedInventoryItem = new InventoryItem();
            inventory.MoveItemServerRPC(t, pos.x, pos.y);
        }
    }

    private void RotateItem()
    {
        if (selectedInventoryItem.itemName != ITEMNAME.NONE)
        {
            inventory.RotateItemServerRPC(selectedInventoryItem);
        }
    }

    private Vector2Int GetGridPostion(Vector2 mousePosition)
    {
        Vector2Int gridPos = Vector2Int.zero;

        gridPos.x = Mathf.FloorToInt((mousePosition.x - rectTransform.position.x) / width);
        gridPos.y = Mathf.FloorToInt((mousePosition.y - rectTransform.position.y) / height);

        return gridPos;
    }

    private void SelectNearItem(ItemUI itemUI)
    {
        selectedNearItem = itemUI;
        selectedNearItem.transform.SetParent(inventoryTile.transform);
    }

    private void PutItem(Vector2Int pos)
    {
        if (selectedNearItem != null)
        {
            GettableItem item = nearDic.ToList().Find(x => x.Value == selectedNearItem).Key;
            inventory.PutItemServerRPC(item.GetComponent<NetworkObject>(), pos.x, pos.y);
            selectedNearItem.transform.SetParent(scrollRect.transform.GetChild(0).GetChild(0));
            selectedNearItem = null;
        }
    }

    private void DropItem(Vector2Int pos)
    {
        if (selectedInventoryItem.itemName != ITEMNAME.NONE && (pos.x < 0 || pos.y < 0 || pos.x >= inventory.sizeX.Value || pos.y >= inventory.sizeY.Value))
        {
            inventory.DropItemServerRPC(selectedInventoryItem);
            selectedInventoryItem = new InventoryItem();
        }
    }
}
