using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct Stat
{
    public int MaxHp;
    public int Hp;
    public float Speed;
    public int Gold;
    public int Damage;
    public int Range;

    public Stat(int maxHp, int hp, 
        float speed, int gold, int damage, int range)
    {
        MaxHp = maxHp;
        Hp = hp;
        Speed = speed;
        Gold = gold;
        Damage = damage;
        Range = range;
    }

    public static Stat operator +(Stat a, Stat b)
    {
        a.MaxHp += b.MaxHp;
        a.Hp += b.Hp;
        a.Speed += b.Speed;
        a.Gold += b.Gold;
        a.Damage += b.Damage;
        a.Range += b.Range;
        return a;
    }

}
