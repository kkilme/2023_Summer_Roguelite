using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestGizmo : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(gameObject.transform.position, 0.01f);
    }
#endif
}
