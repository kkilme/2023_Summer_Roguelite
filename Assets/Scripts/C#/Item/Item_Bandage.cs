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

    public override void Use(Player player)
    {
        player.OnHealed(30);
    }
}
