using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �÷��̾ ȹ���ϴ� ������. ������ ���·� ����
public abstract class GettableItem : MonoBehaviour, IInteraction
{
    [SerializeField] protected ITEM_NAME item; // ȹ���ϴ� ������

    public abstract void Interact(Player player);
    public abstract void InteractComplete(bool bSuccess);
}
