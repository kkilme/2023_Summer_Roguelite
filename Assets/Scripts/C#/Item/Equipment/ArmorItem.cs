using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorItem : Item
{
    public EquipStat EquipStat { get; private set; }

    public ArmorItem() { }
    
    public ArmorItem(ITEMNAME itemName)
    {
        ItemName = itemName;
        switch (itemName)
        {
            case ITEMNAME.TESTHEAD:
                EquipStat = new EquipStat(100, 20);
                break;
            case ITEMNAME.TESTRAREHEAD:
                EquipStat = new EquipStat(100, 40);
                break;
            default:
                EquipStat = new EquipStat();
                break;
        }
    }

    public override bool Use(Player player)
    {
        return player.Equip(this);
    }
}
