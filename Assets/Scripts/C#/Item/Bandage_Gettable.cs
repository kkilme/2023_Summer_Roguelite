using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bandage_Gettable : GettableItem
{
    public override ConditionStruct Interact()
    {
        return condition;
    }

    public override void InteractComplete(bool bSuccess)
    {
 
    }
}
