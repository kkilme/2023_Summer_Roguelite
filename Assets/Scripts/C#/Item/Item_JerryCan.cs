using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_JerryCan : Item
{
    public Item_JerryCan(int count)
    {
        //ItemName = ITEMNAME.JERRY_CAN;
    }

    public override bool Use(Player player)
    {
        return true;
    }
}
