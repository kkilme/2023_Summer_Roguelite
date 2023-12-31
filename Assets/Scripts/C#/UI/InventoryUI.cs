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

public partial class InventoryUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private RectTransform equipRectTransform;
    private const int tileSizeWidth = 64;
    private const int tileSizeHeight = 64;
    [SerializeField]
    private float width;
    [SerializeField]
    private float height;
    [SerializeField]
    private Inventory inventory;

    private ScrollRect scrollRect;
    private GameObject inventoryTile;

    public InventoryItem selectedInventoryItem;
    private ItemUI selectedNearItemUi;
    private GettableItem selectedNearItem;

    //이미지 풀링 담당
    private Stack<ItemUI> inventoryItemUIStack;
    private Stack<ItemUI> nearItemUIStack;

    private Dictionary<InventoryItem, ItemUI> inventoryDic = new Dictionary<InventoryItem, ItemUI>();
    private Dictionary<GettableItem, ItemUI> nearDic = new Dictionary<GettableItem, ItemUI>();

    private void Awake()
    {
        inventoryItemUIStack = new Stack<ItemUI>(transform.GetChild(1).GetComponentsInChildren<ItemUI>(true));
        nearItemUIStack = new Stack<ItemUI>(transform.GetChild(2).GetComponentsInChildren<ItemUI>(true));
        nearItemUIStack.ToList().ForEach(x => x.action += SelectNearItem);
        scrollRect = GetComponentInChildren<ScrollRect>(true);
        inventoryTile = transform.GetChild(1).gameObject;

        width = tileSizeWidth * transform.parent.GetComponent<RectTransform>().localScale.x;
        height = tileSizeWidth * transform.parent.GetComponent<RectTransform>().localScale.y;
    }

    public void Init(Inventory inven)
    {
        inventory = inven;
        rectTransform = transform.GetChild(1).GetComponent<RectTransform>();
        equipRectTransform = transform.GetChild(0).GetComponent<RectTransform>();

        EquipInit();
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

    // 추후에 InputSystem으로 교체.
    private void Update()
    {
        var pos = GetGridPostion(Input.mousePosition);

        if (selectedInventoryItem.itemName != ITEMNAME.NONE)
        {
            inventoryDic[selectedInventoryItem].image.rectTransform.localPosition = new Vector2(pos.x, pos.y) * 64;
        }
        if (selectedNearItemUi != null)
        {
            selectedNearItemUi.image.rectTransform.localPosition = new Vector2(pos.x, pos.y) * 64;
        }
        if (Input.GetMouseButtonDown(0))
        {
            //인벤토리가 클릭됐는지 검사
            selectedInventoryItem = inventory.SelectItem(pos.x, pos.y);
            if (selectedInventoryItem.itemName == ITEMNAME.NONE)
                SelectEquip();
        }
        if (Input.GetMouseButtonDown(1))
        {
            UseItem(pos);
        }
        if (Input.GetMouseButtonUp(0))
        {
            DropItem(pos);
            EquipItem();
            MoveItem(pos);
            PutItem(pos);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }
    }

    // 인벤토리 아이템들을 출력해주는 함수 인벤토리의 이벤트 핸들러를 통해 호출 됨.
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

            if (e.InventoryItems[i].rotationType == ROTATION_TYPE.TOP)
                inventoryDic[e.InventoryItems[i]].image.rectTransform.sizeDelta = new Vector2(e.InventoryItems[i].sizeY, e.InventoryItems[i].sizeX) * 64;
            else
                inventoryDic[e.InventoryItems[i]].image.rectTransform.sizeDelta = new Vector2(e.InventoryItems[i].sizeX, e.InventoryItems[i].sizeY) * 64;
            inventoryDic[e.InventoryItems[i]].image.sprite = GameManager.Resource.TryGetImage(e.InventoryItems[i].itemName);
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

    // GettableItem들을 출력해주는 함수. 인벤토리의 이벤트 핸들러를 통해 호출됨.
    private void DisplayNearItemUI(object sender, Inventory.NearItemEventHandlerArgs e)
    {
        if (e.changedType == Inventory.NearItemEventHandlerArgs.ChangedType.Added)
        {
            if (!nearDic.ContainsKey(e.GettableItem))
            {
                nearDic.Add(e.GettableItem, nearItemUIStack.Pop());
            }
            nearDic[e.GettableItem].image.sprite = GameManager.Resource.TryGetImage(e.GettableItem.ItemName);

            nearDic[e.GettableItem].gameObject.SetActive(true);
            var stat = Item.itemDataDic[e.GettableItem.ItemName];
            
            nearDic[e.GettableItem].image.rectTransform.sizeDelta = new Vector2(stat.sizeX, stat.sizeY) * 64;
            nearDic[e.GettableItem].text.text = e.GettableItem.ItemCount.ToString();
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

    // Enable 시 호출되는 함수. 기존의 DisplayNearItemUI와 기능은 똑같음.
    public void DisplayNearItemUI()
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
            nearDic[nearItems[i]].image.sprite = GameManager.Resource.TryGetImage(nearItems[i].ItemName);
            nearDic[nearItems[i]].gameObject.SetActive(true);
            var stat = Item.itemDataDic[nearItems[i].ItemName];
            nearDic[nearItems[i]].image.rectTransform.sizeDelta = new Vector2(stat.sizeX, stat.sizeY) * 64;
            nearDic[nearItems[i]].text.text = nearItems[i].ItemCount.ToString();
        }
    }

    // 아이템 이동 시키는 함수. 조건 체크 및 기능은 Inventory에서 진행.
    private void MoveItem(Vector2Int pos)
    {
        if (selectedInventoryItem.itemName != ITEMNAME.NONE)
        {
            var t = selectedInventoryItem;
            selectedInventoryItem = new InventoryItem();
            inventory.MoveItemServerRPC(t, pos.x, pos.y);
        }
    }

    // 아이템 회전 시키는 함수. 조건 체크 및 기능은 Inventory에서 진행.
    private void RotateItem()
    {
        if (selectedInventoryItem.itemName != ITEMNAME.NONE)
        {
            inventory.RotateItemServerRPC(selectedInventoryItem);
        }
    }

    // 해당 마우스 좌표를 그리드 좌표로 변환시키는 함수.
    private Vector2Int GetGridPostion(Vector2 mousePosition)
    {
        Vector2Int gridPos = Vector2Int.zero;

        gridPos.x = Mathf.FloorToInt((mousePosition.x - rectTransform.position.x) / width);
        gridPos.y = Mathf.FloorToInt((mousePosition.y - rectTransform.position.y) / height);
        return gridPos;
    }

    // GeattableItem을 선택하는 함수
    private void SelectNearItem(ItemUI itemUI)
    {
        var newUi = inventoryItemUIStack.Pop();
        newUi.gameObject.SetActive(true);

        selectedNearItem = nearDic.ToList().Find(x => x.Value == itemUI).Key;
        selectedNearItemUi = newUi;

        var stat = Item.itemDataDic[selectedNearItem.ItemName];
        newUi.text.text = itemUI.text.text;
        newUi.image.rectTransform.sizeDelta = new Vector2(stat.sizeX, stat.sizeY) * 64;
        newUi.image.sprite = GameManager.Resource.TryGetImage(selectedNearItem.ItemName);
        nearItemUIStack.Push(itemUI);
        itemUI.gameObject.SetActive(false);
    }

    // GettableItem을 인벤토리에 넣는 함수. 조건 체크 및 기능은 Inventory에서 진행
    private void PutItem(Vector2Int pos)
    {
        if (selectedNearItemUi != null)
        {
            if (selectedNearItem != null)
            {
                inventory.PutItemServerRPC(selectedNearItem.GetComponent<NetworkObject>(), pos.x, pos.y);
                inventoryItemUIStack.Push(selectedNearItemUi);
                selectedNearItemUi.gameObject.SetActive(false);
                selectedNearItemUi = null;
                selectedNearItem = null;
            }

            else
            {
                inventoryItemUIStack.Push(selectedNearItemUi);
                selectedNearItemUi.gameObject.SetActive(false);
                selectedNearItemUi = null;
            }
        }
    }

    // 아이템 버리는 함수. 조건 체크 및 기능은 Inventory에서 진행
    private void DropItem(Vector2Int pos)
    {
        if (selectedInventoryItem.itemName != ITEMNAME.NONE && 
            (pos.x < 0 || pos.y < 0 || pos.x >= inventory.sizeX.Value || pos.y >= inventory.sizeY.Value)
            && !(selectedInventoryItem.itemName > ITEMNAME.EQUIPSTART && selectedInventoryItem.itemName < ITEMNAME.EQUIPEND && MouseInEquipUI()))
        {
            inventory.DropItemServerRPC(selectedInventoryItem);
            selectedInventoryItem = new InventoryItem();
        }
    }

    // 아이템 사용 함수. 조건 체크 및 기능은 Inventory에서 진행
    private void UseItem(Vector2Int pos)
    {
        inventory.UseItemServerRPC(pos);
    }
}
