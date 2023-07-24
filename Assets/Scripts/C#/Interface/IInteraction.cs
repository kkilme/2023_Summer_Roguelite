using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteraction
{
    //�������� ������ ����(������ ��� ����, ��� �ð� ��)
    public void Interact(Player player);

    public void InteractComplete(bool bSuccess);
}