using System.Collections;
using System.Collections.Generic;
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
}
