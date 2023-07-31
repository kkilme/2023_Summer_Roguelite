using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LifeShip : NetworkBehaviour, IInteraction
{
    public void Interact(Player player)
    {
        // 인벤토리에 기름통 있는지 체크
        InteractServerRPC();
    }

    public void InteractComplete(bool bSuccess)
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRPC(ServerRpcParams serverRpcParams = default)
    {
        var player = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject.GetComponent<Player>();
        if (player.Inventory.hasItem(ITEMNAME.JERRY_CAN))
        {
            // 아이템 존재
        }
    }
}
