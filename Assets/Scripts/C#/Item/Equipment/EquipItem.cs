using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipItem : Item
{
    public EquipItem(ITEMNAME itemName, ItemStat itemstat)
    {
        ItemStat = itemstat;
        ItemName = itemName;
    }

    public override void Use(Player player)
    {
        throw new System.NotImplementedException();
    }
}
