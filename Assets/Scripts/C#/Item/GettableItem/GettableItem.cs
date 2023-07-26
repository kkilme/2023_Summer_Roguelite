using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어가 획득하는 아이템. 프리팹 형태로 존재
public class GettableItem : MonoBehaviour, IInteraction
{
    [SerializeField] protected ITEMNAME itemName; // 획득하는 아이템
    [SerializeField] protected int itemCount; // 들어있는 아이템 갯수

    public virtual void Interact(Player player)
    {
        player.Inventory.PutItemServerRPC(itemName);
    }
    public virtual void InteractComplete(bool bSuccess)
    {

    }
}
