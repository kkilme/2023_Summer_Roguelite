using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteraction
{
    //�������� ������ ����(������ ��� ����, ��� �ð� ��)
    public ConditionStruct Interact();

    public void InteractComplete(bool bSuccess);
}