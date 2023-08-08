using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//UI는 둘 플레이어가 현재 상호작용할 수 있는가, 현재 바라보고 있는가
//바라보고 있을 때 상호작용 가능 여부를 알린다
public class LifeShip : NetworkBehaviour, IInteraction
{
    private bool _bInteract = false;
    private bool _bFull = false;

    public void Interact(Player player)
    {
        // 인벤토리에 기름통 있는지 체크
        InteractServerRPC();
    }

    public void Interactable(bool bCan)
    {
        //set UI
    }

    public void InteractComplete(bool bSuccess)
    {
        _bInteract = bSuccess;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRPC(ServerRpcParams serverRpcParams = default)
    {
        var player = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject.GetComponent<Player>();

        if (player.Inventory.hasItem(ITEMNAME.JERRY_CAN) && !_bInteract && !_bFull)
        {
            // 아이템 존재
            Debug.Log("기름통 있음");
            _bInteract = true;
            FillUp(player, 300).Forget();
        }

        else
        {
            //errorUI
            Debug.Log("기름통 없음");
        }
    }

    //time 단위는 1당 10밀리초
    private async UniTaskVoid FillUp(Player player, int time = 300)
    {
        int originTime = time;
        while (time > 0 && _bInteract)
        {
            //원래 time - 현재 time을 슬라이드 등으로 표시해서 남은 시간을 알 수 있게
            Debug.Log($" 주유중... {time} / {originTime}");
            --time;
            await UniTask.Delay(TimeSpan.FromMilliseconds(10));
        }

        if (time == 0)
        {
            Debug.Log("사용 완료");
            _bFull = true;
            //플레이어에게 다 찼다고 알리기

            player.CancelInteraction();

        }
    } 
}
