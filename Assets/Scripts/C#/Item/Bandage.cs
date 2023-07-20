using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bandage : Item
{
    [SerializeField] private int healAmount;

    private void Awake()
    {
        itemStat = new ItemStat("Bandage", "Heal Player", null);
    }

    public override void Use(ref Stat stat)
    {
        throw new System.NotImplementedException();
    }

    public override void Use()
    {
        throw new System.NotImplementedException();
    }
}
