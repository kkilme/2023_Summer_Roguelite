using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Creature : MonoBehaviour
{
    protected Stat _stat;

    public abstract void OnDamage(int damage);
}