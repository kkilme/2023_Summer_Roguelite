using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScriptableObjectExtension
{
    // scriptable object 클론
    public static T Clone<T>(this T scriptableObject) where T : ScriptableObject
    {
        if (scriptableObject == null)
        {
            Debug.LogError($"ScriptableObject is null. Returning default {typeof(T)} object.");
            return (T)ScriptableObject.CreateInstance(typeof(T));
        }

        T instance = Object.Instantiate(scriptableObject);
        instance.name = scriptableObject.name; // 이름에서 (Clone) 제거
        return instance;
    }
}
