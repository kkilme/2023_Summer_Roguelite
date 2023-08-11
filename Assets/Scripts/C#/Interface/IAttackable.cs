using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackable
{
    public void OnDamaged(int damage);

    public bool OnHealed(int heal);
}
