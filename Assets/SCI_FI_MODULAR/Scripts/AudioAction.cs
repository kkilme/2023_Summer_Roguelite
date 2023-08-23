using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioAction : ActionBase
{
    AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }
    
    public override void SubAction()
    {
        source.Play();
    }
}
