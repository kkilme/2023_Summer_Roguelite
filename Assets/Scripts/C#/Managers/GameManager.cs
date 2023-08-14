using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Economy;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneName
{
    StartScene,
    LevelScene,
    GameScene,
    TempScene,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } = null;
    public static ResourceManager Resource { get; private set; } = null;

    [SerializeField]
    private Image _fade;

    void Awake()
    {
        if (Instance == null)
            Init();

        else
            Destroy(gameObject);
    }

    private async void Init()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Resource = new ResourceManager();
        Resource.Init();
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await EconomyService.Instance.Configuration.SyncConfigurationAsync();
    }

    public async UniTaskVoid LoadSceneAsync(SceneName next)
    {
        _fade.gameObject.SetActive(true);
        float fadeTime = 0;

        while (fadeTime < 0.99f)
        {
            _fade.color = Color.Lerp(Color.clear, Color.black, fadeTime);
            await UniTask.Delay(TimeSpan.FromMilliseconds(10));
            fadeTime += 0.0167f;
        }

        SceneManager.LoadScene((int)SceneName.TempScene);
        await UniTask.WaitUntil(() => SceneManager.GetActiveScene().buildIndex == (int)SceneName.TempScene);

        AsyncOperation ao = SceneManager.LoadSceneAsync((int)next);
        ao.allowSceneActivation = false;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        await UniTask.WhenAll(UniTask.WaitUntil(() => ao.progress >= 0.89), UniTask.Delay(TimeSpan.FromSeconds(1.5f)));
        ao.allowSceneActivation = true;
        await UniTask.WaitUntil(() => SceneManager.GetActiveScene().buildIndex == (int)next);

        //스테이지에 맞는 맵을 로드해야 함


        fadeTime = 0;
        while (fadeTime < 0.99f)
        {
            _fade.color = Color.Lerp(Color.black, Color.clear, fadeTime);
            await UniTask.Delay(TimeSpan.FromMilliseconds(10));
            fadeTime += 0.0334f;
        }
        _fade.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Resource.Clear();
    }
}
