using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_JerryCan : Item
{
    public Item_JerryCan(int count)
    {
        ItemStat = new ItemStat("Jerry Can", "asdasd", 2, 3, count, 1);
        ItemName = ITEMNAME.JERRY_CAN;
    }

    public override void Use(Player player)
    {
    }
}
