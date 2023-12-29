using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

//UI는 둘 플레이어가 현재 상호작용할 수 있는가, 현재 바라보고 있는가
//바라보고 있을 때 상호작용 가능 여부를 알린다
public class LifeShip : NetworkBehaviour, IInteraction
{
    private bool _bInteract = false;
    private bool _bFull = false;
    private InventoryItem _item;
    private Sequence _fillUpSeq;
    private int _fillTime;
    private int _fillMaxTime;

    public void Interact(Player player)
    {
        // 인벤토리에 기름통 있는지 체크
        //InteractServerRPC();
    }

    public void Interactable(bool bCan)
    {
        //set UI
    }

    public void InteractComplete(bool bSuccess)
    {
        FillUpPauseServerRPC(bSuccess);
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void InteractServerRPC(ServerRpcParams serverRpcParams = default)
    //{
    //    var player = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject.GetComponent<Player>();
    //
    //    if (player.Inventory.HasItem(ITEMNAME.JERRY_CAN, out _item) && !_bInteract && !_bFull && player.ServerLock == PlayerLock.Yes)
    //    {
    //        // 아이템 존재
    //        Debug.Log("기름통 있음");
    //        _bInteract = true;
    //        _fillUpSeq = DOTween.Sequence()
    //        .Append(DOTween.To(() => _fillTime, x => _fillTime = x, _fillMaxTime, 5f).SetEase(Ease.Linear))
    //        .AppendCallback(() =>
    //        {
    //            if (_fillTime == _fillMaxTime)
    //            {
    //                Debug.Log("사용 완료");
    //                _bFull = true;
    //                //플레이어에게 다 찼다고 알리기
    //                player.Inventory.RemoveItem(_item);
    //                _item = new InventoryItem();
    //                player.CancelInteraction();
    //            }
    //        });
    //        //플레이어에게 슬라이더 ui 띄우도록 전달
    //    }
    //
    //    else
    //    {
    //        //errorUI
    //        Debug.Log("기름통 없음");
    //    }
    //}

    [ServerRpc(RequireOwnership = false)]
    private void FillUpPauseServerRPC(bool bSuccess, ServerRpcParams serverRpcParams = default)
    {
        var player = NetworkManager.Singleton.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject.GetComponent<Player>();
        if (player.ServerLock == PlayerLock.No)
        {
            _bInteract = bSuccess;
            _fillUpSeq?.Pause();
            _fillUpSeq = null;
            _fillTime = 0;
        }
    }
}
