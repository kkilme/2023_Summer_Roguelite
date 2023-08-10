using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipItem : Item
{
    public EquipItem(ITEMNAME itemName)
    {
        ItemName = itemName;
    }

    public override void Use(Player player)
    {
        throw new System.NotImplementedException();
    }
}
