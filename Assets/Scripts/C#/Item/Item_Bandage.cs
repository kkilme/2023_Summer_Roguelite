using System.Collections;
using System.Collections.Generic;

public class Item_Bandage : Item
{
    private int healAmount;

    public Item_Bandage()
    {
        ItemName = ITEMNAME.BANDAGE;
        healAmount = 30;
    }

    public override bool Use(Player player)
    {
        return player.OnHealed(30);
    }
}
