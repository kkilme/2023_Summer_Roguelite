using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ToggleAnimAction : MonoBehaviour
{
    Animator anim;
    private int _animHash;
    private int _count;

    void Start()
    {
        anim = GetComponent<Animator>();
        _animHash = Animator.StringToHash("Open");
        _count = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.CompareTo("Player") == 0 && !anim.GetBool(_animHash))
        {
            anim.SetBool(_animHash, true);
            ++_count;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.CompareTo("Player") == 0)
        {
            --_count;
            if (_count == 0)
                anim.SetBool(_animHash, false);
        }
    }
}
