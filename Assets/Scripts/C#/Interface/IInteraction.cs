using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteraction
{
    //�������� ������ ����(������ ��� ����, ��� �ð� ��)
    public void Interact();

    public void InteractComplete(bool bSuccess);
}