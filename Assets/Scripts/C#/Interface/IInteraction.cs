using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteraction
{
    //아이템의 정보를 전달(아이템 사용 조건, 사용 시간 등)
    public void Interact(Player player);

    public void Interactable(bool bCan);

    public void InteractComplete(bool bSuccess);
}