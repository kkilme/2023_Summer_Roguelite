using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public Stat PlayerStat { get; private set; }
    public Inventory Inventory { get; private set; }

    [ServerRpc]
    public void SetPlayerStatServerRPC(Stat stat)
    {
        PlayerStat = stat;
    }
}
