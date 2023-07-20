using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat
{
    public int MaxHp;
    public int Hp;
    public float Speed;
    public int Gold;
    public int Damage;
    public int Range;

    public Stat()
    {
        MaxHp = 0;
        Hp = 0;
        Speed = 0;
        Gold = 0;
        Damage = 0;
        Range = 0;
    }

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
}
