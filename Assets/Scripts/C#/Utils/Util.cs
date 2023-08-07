using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Util
{
    [System.Diagnostics.Conditional("ENABLE_LOG")]
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T component = null;
        go.TryGetComponent(out component);

        if (component == null)
        {
            Log($"{nameof(T)} is not exist {go.name}");
            component = go.AddComponent<T>();
        }

        return component;
    }

    public static int GetRealHashCode()
    {
        // 임시시시
        return DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;
    }

}
