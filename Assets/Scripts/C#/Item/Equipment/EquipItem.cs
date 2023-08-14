using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipItem : Item
{
    private EquipStat _equipStat;

    public EquipItem() { }
    
    public EquipItem(ITEMNAME itemName)
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
        return player.EquipArmor(ItemName, _equipStat);
    }
}
