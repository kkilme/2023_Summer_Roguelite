using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } = null;
    public static ResourceManager Resource { get; private set; } = null;

    void Awake()
    {
        if (Instance == null)
            Init();

        else
            Destroy(gameObject);
    }

    private void Init()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Resource = new ResourceManager();
        Resource.Init();
    }

    private void OnDestroy()
    {
        Resource.Clear();
    }
}
