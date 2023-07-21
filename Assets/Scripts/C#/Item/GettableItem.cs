using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어가 획득하는 아이템. 프리팹 형태로 존재
public abstract class GettableItem : MonoBehaviour, IInteraction
{
    [SerializeField] protected ITEM_NAME item; // 획득하는 아이템

    public abstract void Interact(Player player);
    public abstract void InteractComplete(bool bSuccess);
}
