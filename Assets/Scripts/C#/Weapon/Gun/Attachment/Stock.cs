using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Stock", menuName = "Weapon/Attachment/Stock")]
public class Stock : ScriptableAttachment
{
    public float recoilFixRate;


    public override void ApplyAttachmentEffect()
    {
        
    }

    public override void RemoveAttachmentEffect()
    {
        throw new System.NotImplementedException();
    }

}
