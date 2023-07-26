using System.Collections;
using System.Collections.Generic;
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
    private Item selectedItem;

    [SerializeField]
    private Image[] itemImages;
    private TextMeshProUGUI[] texts;

    private Dictionary<Item, Image> itemImageDic = new Dictionary<Item, Image>();

    private void Start()
    {
        inventory = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Inventory>();
        texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        inventory.OnInventoryChanged += DisplayInventoryUI;
        rectTransform = GetComponent<RectTransform>();

        width = tileSizeWidth * transform.parent.GetComponent<RectTransform>().localScale.x;
        height = tileSizeWidth * transform.parent.GetComponent<RectTransform>().localScale.y;
    }

    private void Update()
    {
        if (selectedItem != null)
        {
            var pos = GetGridPostion(Input.mousePosition);
            itemImageDic[selectedItem].rectTransform.localPosition = new Vector2(pos.x, pos.y) * 64;
        }
        if (Input.GetMouseButtonDown(0))
        {
            SelectItem(GetGridPostion(Input.mousePosition));
        }
        if (Input.GetMouseButtonUp(0))
        {
            MoveItem(GetGridPostion(Input.mousePosition));
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Inventory>().PutItemServerRPC(ITEMNAME.BANDAGE);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Inventory>().PutItemServerRPC(ITEMNAME.GAUGE_12, ROTATION_TYPE.TOP, Random.Range(2, 9));
        }
    }

    private void DisplayInventoryUI(object sender, Inventory.InventoryEventHandlerArgs e)
    {
        itemImageDic.Clear();

        for (int i = 0; i < e.Items.Count; i++)
        {
            itemImages[i].gameObject.SetActive(true);
            //itemImages[i] = e.Items[i].ItemStat.image;
            //itemImages[i].SetNativeSize();

            if (e.ItemRotationDic[e.Items[i]].Equals(ROTATION_TYPE.TOP))
                itemImages[i].rectTransform.sizeDelta = new Vector2(e.Items[i].ItemStat.sizeY, e.Items[i].ItemStat.sizeX) * 64;
            else
                itemImages[i].rectTransform.sizeDelta = new Vector2(e.Items[i].ItemStat.sizeX, e.Items[i].ItemStat.sizeY) * 64;

            texts[i].text = e.Items[i].ItemStat.currentCount.ToString();

            if (selectedItem != null)
            {
                if (e.Items[i] != selectedItem)
                    itemImages[i].rectTransform.localPosition = new Vector2(e.ItemPositionDic[e.Items[i]].x, e.ItemPositionDic[e.Items[i]].y) * 64;
            }
            else
                itemImages[i].rectTransform.localPosition = new Vector2(e.ItemPositionDic[e.Items[i]].x, e.ItemPositionDic[e.Items[i]].y) * 64;

            itemImageDic.Add(e.Items[i], itemImages[i]);
        }

        for (int i = e.Items.Count; i < itemImages.Length; i++)
        {
            itemImages[i].gameObject.SetActive(false);
        }
    }

    private void SelectItem(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= inventory.InventorySpace.GetLength(0) || pos.y >= inventory.InventorySpace.GetLength(1))
        {
            return;
        }

        selectedItem = inventory.InventorySpace[pos.x, pos.y];
    }

    private void MoveItem(Vector2Int pos)
    {
        if (selectedItem != null)
        {
            var t = selectedItem;
            selectedItem = null;
            inventory.MoveItem(t, pos.x, pos.y);
        }
    }

    private void RotateItem()
    {
        if (selectedItem != null)
        {
            inventory.RotateItem(selectedItem);
        }
    }

    private Vector2Int GetGridPostion(Vector2 mousePosition)
    {
        Vector2Int gridPos = Vector2Int.zero;

        gridPos.x = (int)((mousePosition.x - rectTransform.position.x) / width);
        gridPos.y = (int)((mousePosition.y - rectTransform.position.y) / height);

        return gridPos;
    }
}
