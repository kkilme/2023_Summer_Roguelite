using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bandage : Item
{
    [SerializeField] private int healAmount;
    [SerializeField] private Stat healStat;

    private void Awake()
    {
        itemStat = new ItemStat("Bandage", "Heal Player", null);
    }

    public override void Use(ref Stat stat)
    {
        stat += healStat;
    }
}
