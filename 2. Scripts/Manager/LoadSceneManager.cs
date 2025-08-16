using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : Singleton<LoadSceneManager>
{
    public event Action<float> OnLoadingProgressChanged;             // 로딩 퍼센트 변경 알림 이벤트
    public float               LoadingProgress { get; private set; } // 현재 로딩 퍼센트

    public Action OnLoadingCompleted;

    protected override void Awake()
    {
        base.Awake();
    }

    public void LoadScene(string sceneName, Action onComplete = null)
    {
        // 로딩창
        GameManager.Instance.TimeScaleSetDefault();
        GameManager.Instance.TimeScaleMultiplierUpdate();
        UIManager.Instance.CloseAll();
        LoadingScreenController.Instance.Show();
        LoadAssetManager.Instance.ReleaseAudioClips();
        StartCoroutine(InternalLoadScene(sceneName, LoadSceneMode.Single, onComplete));
    }

    // 씬을 현재 씬에 Additive 방식으로 추가 로드
    public void LoadSceneAdditive(string sceneName, Action onComplete = null)
    {
        StartCoroutine(InternalLoadScene(sceneName, LoadSceneMode.Additive, onComplete));
    }

    // 내부 공통 처리 (Single or Additive)
    private IEnumerator InternalLoadScene(string sceneName, LoadSceneMode mode, Action onComplete)
    {
        //  Single 모드 → UI 루트 재초기화

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, mode);
        asyncLoad.allowSceneActivation = false;


        while (!asyncLoad.isDone)
        {
            LoadingProgress = asyncLoad.progress;
            OnLoadingProgressChanged?.Invoke(LoadingProgress);

            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }


            yield return null;
        }

        // if (mode == LoadSceneMode.Single)
        // {
        //     UIManager.Instance.InitializeUIRoot();
        // }


        // 후처리 콜백 실행 (ex: 대사 출력)
        OnLoadingCompleted?.Invoke();
        onComplete?.Invoke();
    }
}