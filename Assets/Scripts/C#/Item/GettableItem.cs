using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �÷��̾ ȹ���ϴ� ������. ������ ���·� ����
public abstract class GettableItem : MonoBehaviour, IInteraction
{
    [SerializeField] protected ITEM_NAME item; // ȹ���ϴ� ������
    [SerializeField] protected ConditionStruct condition; // �ش� �������� ȹ���ϴµ��� �ʿ��� ����

    public virtual ConditionStruct Interact() { return  condition; }
    public abstract void InteractComplete(bool bSuccess);
}
