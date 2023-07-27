using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class Player : NetworkBehaviour, IAttackable
{
    public Stat PlayerStat { get => _playerStat; }
    private Stat _playerStat;
    public Inventory Inventory { get; private set; }
    private PlayerController _playerController;

    [ServerRpc]
    public void SetPlayerStatServerRPC(Stat stat)
    {
        _playerStat = stat;
    }

    public override void OnNetworkSpawn()
    {
        _playerController = new PlayerController(gameObject, IsOwner);
        _playerStat = new Stat(5,5,5,5,5,5);
        Inventory = GetComponent<Inventory>();
        FindObjectOfType<Canvas>().gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        transform.position += Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * _playerController.MoveDir * Time.deltaTime * PlayerStat.Speed;
        Debug.Log(PlayerStat.Speed);
    }

    public void OnDamaged(int damage)
    {
        _playerStat.Hp -= damage;
        //사운드
    }

    public void OnHealed(int heal)
    {
        _playerStat.Hp = _playerStat.Hp + heal < _playerStat.MaxHp ? _playerStat.Hp + heal : _playerStat.MaxHp;
    }

    public override void OnNetworkDespawn()
    {
        _playerController.Clear();
    }
}
