using Cysharp.Threading.Tasks.Triggers;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
using Unity.Netcode;
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

    private Item selectedInventoryItem;
    private ItemUI selectedNearItem;

    private Stack<ItemUI> inventoryItemUIStack;
    private Stack<ItemUI> nearItemUIStack;

    private Dictionary<Item, ItemUI> inventoryDic = new Dictionary<Item, ItemUI>();
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
    }

    private void OnDisable()
    {
        inventory.OnInventoryChanged -= DisplayInventoryUI;
        inventory.OnNearItemChanged -= DisplayNearItemUI;
    }

    private void Update()
    {
        if (selectedInventoryItem != null)
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
            MoveItem(GetGridPostion(Input.mousePosition));
            PutItem(GetGridPostion(Input.mousePosition));
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventory.PutItemServerRPC(ITEMNAME.BANDAGE);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            inventory.PutItemServerRPC(ITEMNAME.GAUGE_12, ROTATION_TYPE.TOP, Random.Range(2, 9));
        }
    }

    private void DisplayInventoryUI(object sender, Inventory.InventoryEventHandlerArgs e)
    {
        // 인벤토리에서 제거된 아이템 추출 및 삭제
        var removedItems = inventoryDic.Keys.Except(e.Items).ToArray();

        for (int i = 0; i < removedItems.Length; i++)
        {
            inventoryItemUIStack.Push(inventoryDic[removedItems[i]]);
            inventoryDic[removedItems[i]].gameObject.SetActive(false);
            inventoryDic.Remove(removedItems[i]);
        }

        for (int i = 0; i < e.Items.Count; i++)
        {
            if (!inventoryDic.ContainsKey(e.Items[i]))
            {
                inventoryDic.Add(e.Items[i], inventoryItemUIStack.Pop());
            }

            inventoryDic[e.Items[i]].gameObject.SetActive(true);
            //itemImages[i] = e.Items[i].ItemStat.image;
            //itemImages[i].SetNativeSize();

            if (e.ItemRotationDic[e.Items[i]].Equals(ROTATION_TYPE.TOP))
                inventoryDic[e.Items[i]].image.rectTransform.sizeDelta = new Vector2(e.Items[i].ItemStat.sizeY, e.Items[i].ItemStat.sizeX) * 64;
            else
                inventoryDic[e.Items[i]].image.rectTransform.sizeDelta = new Vector2(e.Items[i].ItemStat.sizeX, e.Items[i].ItemStat.sizeY) * 64;

            inventoryDic[e.Items[i]].text.text = e.Items[i].ItemStat.currentCount.ToString();

            if (selectedInventoryItem != null)
            {
                if (e.Items[i] != selectedInventoryItem)
                    inventoryDic[e.Items[i]].image.rectTransform.localPosition = new Vector2(e.ItemPositionDic[e.Items[i]].x, e.ItemPositionDic[e.Items[i]].y) * 64;
            }
            else
                inventoryDic[e.Items[i]].image.rectTransform.localPosition = new Vector2(e.ItemPositionDic[e.Items[i]].x, e.ItemPositionDic[e.Items[i]].y) * 64;
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
        if (selectedInventoryItem != null)
        {
            var t = selectedInventoryItem;
            selectedInventoryItem = null;
            inventory.MoveItem(t, pos.x, pos.y);
        }
    }

    private void RotateItem()
    {
        if (selectedInventoryItem != null)
        {
            inventory.RotateItem(selectedInventoryItem);
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
            inventory.PutItemServerRPC(item.ItemName, pos.x, pos.y, ROTATION_TYPE.RIGHT, item.ItemCount);
            selectedNearItem.transform.SetParent(scrollRect.transform.GetChild(0).GetChild(0));
            selectedNearItem = null;
            item.DespawnServerRPC();
        }
    }
}
