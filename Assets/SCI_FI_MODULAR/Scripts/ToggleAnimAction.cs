using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ToggleAnimAction :  ActionBase
{
    public string variableName;
    Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public override void SubAction()
    {
        bool status = anim.GetBool(variableName);
        anim.SetBool(variableName, !status);
    }
}
