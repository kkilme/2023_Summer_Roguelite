using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleObjectAction : MonoBehaviour
{
    [HideInInspector]
    public bool ignoreNext;

    public void Action()
    {
        if(!ignoreNext)
            gameObject.SetActive(!gameObject.activeSelf);
        ignoreNext = false;
    }
}
