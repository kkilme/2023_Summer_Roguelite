using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorItem : Item
{
    public ArmorItem() { }
    private EquipStat _equipStat;
    
    public ArmorItem(ITEMNAME itemName)
    {
        ItemName = itemName;

        switch (itemName)
        {
            case ITEMNAME.TESTHEAD:
                _equipStat = new EquipStat(100, 20);
                break;
            case ITEMNAME.TESTRAREHEAD:
                _equipStat = new EquipStat(100, 40);
                break;
            default:
                _equipStat = new EquipStat();
                break;
        }
    }

    public override bool Use(Player player)
    {
        return player.Equip(ItemName, _equipStat);
    }
}