using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GettableItem : MonoBehaviour, IInteraction
{
    [SerializeField] protected Item item;
    [SerializeField] protected ConditionStruct condition;

    public abstract ConditionStruct Interact();
    public abstract void InteractComplete(bool bSuccess);
}
