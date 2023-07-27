using System.Collections;
using System.Collections.Generic;

public class Item_Bandage : Item
{
    private int healAmount;
    private Stat healStat;

    public Item_Bandage(int count)
    {
        ItemStat = new ItemStat("Bandage", "Heal Player", null, 1, 1, count, 5);
        healStat = new Stat();
        ItemName = ITEMNAME.BANDAGE;
        healStat.Hp = healAmount;
    }

    public override void Use(Player player)
    {
        player.SetPlayerStatServerRPC(player.PlayerStat + healStat);
    }
}
