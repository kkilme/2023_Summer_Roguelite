using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    private Stat _stat;

    public void Init(Stat stat)
    {
        _stat = stat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GOTag.Player.ToString()))
        {
            IAttackable attackable;
            other.TryGetComponent(out attackable);
            attackable.OnDamaged(_stat.Damage);
        }
    }
}
