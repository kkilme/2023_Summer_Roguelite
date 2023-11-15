using System.Collections;
using System.Collections.Generic;

public class Item_CannedFood : Item
{
    private int healAmount;

    public Item_CannedFood()
    {
        ItemName = ITEMNAME.CANNEDFOOD;
        healAmount = 30;
    }

    public override bool Use(Player player)
    {
        return player.OnHealed(healAmount);
    }
}
