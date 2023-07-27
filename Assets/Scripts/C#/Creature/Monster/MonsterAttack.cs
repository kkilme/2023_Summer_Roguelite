using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    private Stat _stat;

    public void Init(Stat stat)
    {
        _stat = stat;
        GetComponent<BoxCollider>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GOTag.Player.ToString()))
        {
            IAttackable attackable;
            other.TryGetComponent(out attackable);
            if (attackable != null)
                attackable.OnDamaged(_stat.Damage);
        }
    }
}
