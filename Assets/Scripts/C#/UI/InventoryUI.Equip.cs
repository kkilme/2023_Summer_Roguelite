using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public partial class InventoryUI : MonoBehaviour
{
    [SerializeField]
    private ItemUI[] equipItemUI;

    private Vector2[,] equipUISize;

    private float equipUIMaxX;
    private float equipUIMinX;
    private float equipUIMaxY;
    private float equipUIMinY;

    private void EquipInit()
    {
        //Equip UI AABB
        equipUIMaxX = equipRectTransform.position.x + equipRectTransform.sizeDelta.x / 2;
        equipUIMinX = equipRectTransform.position.x - equipRectTransform.sizeDelta.x / 2;
        equipUIMaxY = equipRectTransform.position.y + equipRectTransform.sizeDelta.y / 2;
        equipUIMinY = equipRectTransform.position.y - equipRectTransform.sizeDelta.y / 2;

        equipUISize = new Vector2[equipItemUI.Length, 2];

        for (int i = 0; i < equipItemUI.Length; ++i)
        {
            RectTransform rect = equipItemUI[i].GetComponent<RectTransform>();
            equipUISize[i, 0] = rect.position;
            equipUISize[i, 1] = equipUISize[i, 0] + rect.sizeDelta;

        }
    }

    private bool MouseInEquipUI()
    {
        Vector2 mousePos = Input.mousePosition;
        if (mousePos.x > equipUIMaxX || mousePos.x < equipUIMinX || mousePos.y > equipUIMaxY || mousePos.y < equipUIMinY)
            return false;

        return true;
    }

    private void SelectEquip()
    {
        Vector2 input = Input.mousePosition;

        for (int i = 0; i < equipItemUI.Length; ++i) 
        {
            if (input.x >= equipUISize[i, 0].x && input.x <= equipUISize[i, 1].x 
                && input.y >= equipUISize[i, 0].y && input.y <= equipUISize[i, 1].y)
            {
                selectedInventoryItem = inventory.SelectEquip((ITEMNAME)(1000 + i * 100));
                equipItemUI[i].gameObject.SetActive(false);
                break;
            }
        }
    }

    private void EquipItem()
    {
        if (selectedInventoryItem.itemName > ITEMNAME.EQUIPSTART && selectedInventoryItem.itemName < ITEMNAME.EQUIPEND && MouseInEquipUI())
        {
            //장착...
            var t = selectedInventoryItem;
            selectedInventoryItem = new InventoryItem();
            inventory.EquipServerRPC(t);
        }
    }

    [ClientRpc]
    public void EquipUISetClientRpc(InventoryItem item, ClientRpcParams clientRpcParams = default)
    {
        switch (item)
        {
            case var _ when item.itemName > ITEMNAME.EQUIPSTART && item.itemName < ITEMNAME.WEAPONEND:
                equipItemUI[0].gameObject.SetActive(true);
                break;
            case var _ when item.itemName > ITEMNAME.WEAPONEND && item.itemName < ITEMNAME.SUBWEAPONEND:
                equipItemUI[1].gameObject.SetActive(true);
                break;
            case var _ when item.itemName > ITEMNAME.SUBWEAPONEND && item.itemName < ITEMNAME.HEADEND:
                equipItemUI[2].gameObject.SetActive(true);
                break;
            case var _ when item.itemName > ITEMNAME.HEADEND && item.itemName < ITEMNAME.CLOTHEND:
                equipItemUI[3].gameObject.SetActive(true);
                break;
        };
    }
}
