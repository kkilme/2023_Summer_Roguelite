using System.Collections;
using System.Collections.Generic;

public class Item_Bandage : Item
{
    private int healAmount;
    private Stat healStat;

    public Item_Bandage()
    {
        itemStat = new ItemStat("Bandage", "Heal Player", null, 1, 1);
    }

    public override void Use(ref Stat stat)
    {
        stat += healStat;
    }
}
