using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    LODGroup _lodGroup;

    private void Awake()
    {
        _lodGroup = transform.parent.GetComponent<LODGroup>();
        Debug.Log(gameObject.layer);
    }
}
