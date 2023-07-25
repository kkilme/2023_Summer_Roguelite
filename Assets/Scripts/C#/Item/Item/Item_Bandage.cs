using System.Collections;
using System.Collections.Generic;

public class Item_Bandage : Item
{
    private int healAmount;
    private Stat healStat;

    public Item_Bandage()
    {
        itemStat = new ItemStat("Bandage", "Heal Player", null, 1, 1);
        healStat = new Stat();
        ItemName = ITEMNAME.BANDAGE;
        healStat.Hp = healAmount;
    }

    public override void Use(Player player)
    {
        player.SetPlayerStatServerRPC(player.PlayerStat + healStat);
    }
}
