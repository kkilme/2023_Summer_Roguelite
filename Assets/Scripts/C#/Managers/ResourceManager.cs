using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ResourceManager
{ 
    private Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();

    public void Init()
    {
        LoadAsyncAll<GameObject>("Prefabs");
    }

    public T GetObject<T>(string name) where T : UnityEngine.Object
    {
        if (_resources.ContainsKey(name))
            return _resources[name] as T;

        Debug.LogError("Fail to Get Asset!");
        return null;
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
            _resources.Add(name, op.Result);
            callback?.Invoke(op.Result);
            Addressables.Release<T>(asyncOperation);
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
                    ++loadCount;
                    Debug.Log($"Load {loadCount} / {totalCount}");
                    if (loadCount == totalCount)
                        MapManager.Instance.MonsterInit();
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
