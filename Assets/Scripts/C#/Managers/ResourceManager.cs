using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class ResourceManager
{ 
    private Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();
    private Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
    private Dictionary<string, Sprite> _images = new Dictionary<string, Sprite>();

    public void Init()
    {
        LoadAsyncAll<GameObject>("Prefabs");
        LoadAsyncAll<Sprite>("Images");
    }

    public Sprite TryGetImage(ITEMNAME name)
    {
        bool b = _images.ContainsKey($"Image/{name}.PNG");
        if (b)
        {
            return _images[$"Image/{name}.PNG"];
        }
        else
        {
            return _images["Image/Item Image.png"];
        }
    }

    public T GetObject<T>(string name) where T : UnityEngine.Object
    {
        if (_prefabs.ContainsKey($"{name}.prefab"))
            return _prefabs[$"{name}.prefab"] as T;

        Debug.LogError($"Fail to Get {name}.prefab Asset!");
        return null;
    }

    public List<T> GetAll<T>(string name) where T : UnityEngine.Object
    {
        List<T> objs = new List<T>();
        GameObject go;
        foreach (var key in _prefabs.Keys)
        {
            if (key.Contains(name))
            {
                go = _prefabs[key];
                objs.Add(go.GetComponent<T>());
            }
        }
        return objs;
    }

    private void LoadAsync<T>(string name, Action<T> callback = null) where T : UnityEngine.Object
    {
        if (_resources.TryGetValue(name, out var obj))
        {
            callback?.Invoke(obj as T);
            return;
        }

        var asyncOperation = Addressables.LoadAssetAsync<T>(name);
        asyncOperation.Completed += (op) =>
        {
            if (typeof(T) == typeof(GameObject))
                _prefabs.Add(name, op.Result as GameObject);
            else if (typeof(T) == typeof(Sprite))
                _images.Add(name, op.Result as Sprite);
            else
            {
                Debug.Log(typeof(T));
            }
            callback?.Invoke(op.Result);
            Addressables.Release(asyncOperation);
        };
    }

    //전투 시작 시 방, 총알, 몬스터
    private void LoadAsyncAll<T>(string name) where T : UnityEngine.Object
    {
        var asyncOperation = Addressables.LoadResourceLocationsAsync(name, typeof(T));
        asyncOperation.Completed += (op) =>
        {
            int loadCount = 0;
            int totalCount = op.Result.Count;

            for (int i = 0; i < totalCount; ++i)
            {
                LoadAsync<T>(op.Result[i].PrimaryKey, (obj) =>
                {
                    //Debug.Log($"prefabs: {_prefabs.Count}, _images: {_images.Count}");
                });
            }

            Addressables.Release(asyncOperation);
        };
    }

    public void Clear()
    {
        _resources.Clear();
    }
}
