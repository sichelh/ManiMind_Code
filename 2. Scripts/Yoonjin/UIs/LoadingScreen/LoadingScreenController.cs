using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingScreenController : Singleton<LoadingScreenController>
{
    public event Action OnLoadingComplete; // 로딩 완료 시점을 알리는 이벤트 
    private Image overlayImage;            // 검은 화면 처리용 이미지
    private Slider progressSlider;         // 로딩  슬라이더

    [SerializeField] private float minLoadTime = 1.5f;          // 최소 로딩 시간
    [SerializeField] private float extraDelayAfterReady = 0.4f; // 로딩 완료 후 추가 여유 시간
    private float currentProgress = 0f;

    protected override void Awake()
    {
        base.Awake();

        // 프리팹이 없다면 Resources에서 로드해서 자식으로 붙임
        if (overlayImage == null || progressSlider == null)
        {
            GameObject prefab = Resources.Load<GameObject>("UI/LoadingCanvas");
            GameObject uiRoot = Instantiate(prefab, transform); // 자식으로 붙임
            progressSlider = uiRoot.GetComponentInChildren<Slider>();
            overlayImage = uiRoot.transform.GetChild(1).GetComponent<Image>();
        }

        gameObject.SetActive(false);
    }

    // 로딩 시작. LoadSceneManager의 LoadScene에서 호출
    public void Show()
    {
        StopAllCoroutines();

        gameObject.SetActive(true);
        progressSlider.value = 0f;
        currentProgress = 0f;

        LoadSceneManager.Instance.OnLoadingProgressChanged -= OnProgressChanged;
        LoadSceneManager.Instance.OnLoadingProgressChanged += OnProgressChanged;

        StartCoroutine(LoadingRoutine());
    }

    private void OnProgressChanged(float value)
    {
        currentProgress = value;
    }


    private IEnumerator LoadingRoutine()
    {
        // 1. 페이드인
        overlayImage.color = new Color(0, 0, 0, 1);
        yield return overlayImage.DOFade(0f, 0.25f).WaitForCompletion();

        float startTime = Time.time;
        currentProgress = 0f;
        float displayedProgress = 0f;

        // 2. 실제 progress 따라 슬라이더 부드럽게 증가
        while (displayedProgress < currentProgress)
        {
            displayedProgress = Mathf.MoveTowards(displayedProgress, currentProgress, Time.deltaTime * 0.5f);
            progressSlider.value = displayedProgress;
            yield return null;
        }

        // 3. 남은 구간 (0.9 → 1.0) 채우기
        while (displayedProgress < 1f)
        {
            displayedProgress = Mathf.MoveTowards(displayedProgress, 1f, Time.deltaTime * 1f);
            progressSlider.value = displayedProgress;
            yield return null;
        }

        // 4. 최소 로딩 시간 보장
        float elapsed = Time.time - startTime;
        if (elapsed < minLoadTime)
        {
            yield return new WaitForSeconds(minLoadTime - elapsed);
        }

        // 5. 로딩 완료 후 여유 시간
        yield return new WaitForSeconds(extraDelayAfterReady);

        // 6. 이벤트 해제
        LoadSceneManager.Instance.OnLoadingProgressChanged -= OnProgressChanged;

        // !!로딩 완료!!
        OnLoadingComplete?.Invoke();

        // 7. 검정 화면 덮기 (페이드 아웃)
        yield return overlayImage.DOFade(1f, 0.25f).WaitForCompletion();

        // 8. 로딩 UI 숨김
        gameObject.SetActive(false);
    }
}