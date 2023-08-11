using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InventoryUI : MonoBehaviour
{
    private ItemUI weaponItemUI;
    private ItemUI subWeaponItemUI;
    private ItemUI headItemUI;
    private ItemUI clothItemUI;

    private float equipUIMaxX;
    private float equipUIMinX;
    private float equipUIMaxY;
    private float equipUIMinY;

    private bool MouseInEquipUI()
    {
        Vector2 mousePos = Input.mousePosition;
        if (mousePos.x > equipUIMaxX || mousePos.x < equipUIMinX || mousePos.y > equipUIMaxY || mousePos.y < equipUIMinY)
            return false;

        return true;
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
}
