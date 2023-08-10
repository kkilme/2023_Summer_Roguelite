using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InventoryUI : MonoBehaviour
{
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
            inventory.EquipItem(selectedInventoryItem);
            selectedInventoryItem = new InventoryItem();
        }
    }
}
