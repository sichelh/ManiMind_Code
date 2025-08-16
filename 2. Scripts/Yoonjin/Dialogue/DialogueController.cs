using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueController : Singleton<DialogueController>
{
    private DialogueGroupSO currentGroup;
    private int currentLineIndex = 0;

    private OverlayDialogueUI overlayUI;
    private TutorialDialogueUI tutorialUI;

    private IDialogueUI activeUI; // 현재 활성 UI (타자/스킵 제어)

    // 읽은 그룹 키들을 저장
    private HashSet<string> readGroups = new();

    [Header("디버깅 중 대사 스킵")]
    [SerializeField] private bool dialogueSkip = false;


    private Action OnCallBackAction;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        // 씬 로드 완료 후 자동 호출될 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 이벤트 해제 (중복 실행, 메모리 누수 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 완전히 로드된 직후 호출되는 콜백
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Additive로 DialogueScene이 로드되었을 때만 실행
        if (currentGroup != null &&
            currentGroup.mode == DialogueMode.Fullscreen &&
            scene.name == "DialogueScene")
        {
            ShowCurrentLine();
        }
    }

    // 특정 그룹 키의 대사 재생 시작
    public void Play(string groupKey, Action callback = null)
    {
        if (dialogueSkip || readGroups.Contains(groupKey))
        {
            Debug.Log("스킵됨");
            callback?.Invoke();
            return;
        }


        DialogueGroupTable table = TableManager.Instance.GetTable<DialogueGroupTable>();
        DialogueGroupSO    group = table.GetDataByID(groupKey);

        if (group == null)
        {
            Debug.LogError($"GroupKey '{groupKey}'를 찾을 수 없습니다.");
            return;
        }

        currentGroup = group;
        currentLineIndex = 0;

        if (group.mode == DialogueMode.Fullscreen)
        {
            // DialogueScene을 현재 씬 위에 Additive로 로드
            LoadSceneManager.Instance.LoadSceneAdditive("DialogueScene");
        }
        else
        {
            ShowCurrentLine();
        }

        OnCallBackAction = callback;
    }

    // 다음 대사 줄로 이동
    public void Next()
    {
        if (currentGroup == null)
        {
            return;
        }

        currentLineIndex++;
        ShowCurrentLine();
    }

    // 현재 대사 줄을 출력
    private void ShowCurrentLine()
    {
        if (currentGroup == null || currentLineIndex >= currentGroup.lines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentGroup.lines[currentLineIndex];

        if (currentGroup.mode == DialogueMode.Overlay)
        {
            OverlayDialogueUI ui = GetOrCreateUI<OverlayDialogueUI>("UI/OverlayDialogueUI");
            ui.gameObject.SetActive(true); // UI 활성화
            activeUI = ui as IDialogueUI;
            activeUI?.Show(line, withTyping: true);
        }
        else if (currentGroup.mode == DialogueMode.Tutorial)
        {
            TutorialDialogueUI ui = GetOrCreateUI<TutorialDialogueUI>("UI/TutorialDialogueUI");
            ui.gameObject.SetActive(true);
            activeUI = ui as IDialogueUI;
            activeUI?.Show(line, withTyping: true);
        }
        else
        {
            FullscreenDialogueUI fullscreenUI = FindFirstObjectByType<FullscreenDialogueUI>(FindObjectsInactive.Include);
            activeUI = fullscreenUI as IDialogueUI;
            activeUI?.Show(line, withTyping: true);
        }
    }

    // 대사 종료 처리
    public void EndDialogue()
    {
        // 다 읽은 대사는 스킵
        if (currentGroup != null && currentGroup.mode != DialogueMode.Tutorial)
        {
            readGroups.Add(currentGroup.groupKey);
        }

        if (currentGroup.mode == DialogueMode.Fullscreen)
        {
            // 현재 DialogueScene을 언로드 → 원래 씬 복귀
            SceneManager.UnloadSceneAsync("DialogueScene");
        }
        else if (currentGroup.mode == DialogueMode.Overlay)
        {
            overlayUI?.gameObject.SetActive(false); // UI 비활성화
        }
        else
        {
            tutorialUI?.gameObject.SetActive(false);
        }

        activeUI = null; // 해제: 다음 대사에서 새 UI가 설정됨

        currentGroup = null;
        currentLineIndex = 0;
        OnCallBackAction?.Invoke();
        EventBus.Publish("DialogueFinished");
    }

    public bool TryCompleteTyping()
    {
        // 타이핑 중이면 즉시 완료
        if (activeUI != null && activeUI.IsTyping)
        {
            activeUI.CompleteTyping();
            return true;
        }
        return false;
    }

    // 오버레이나 튜토리얼 프리팹을 찾고, 없으면 생성
    private T GetOrCreateUI<T>(string resourcePath) where T : MonoBehaviour
    {
        // 이미 참조된 OverlayDialogueUI가 있으면 그대로 반환
        if (typeof(T) == typeof(OverlayDialogueUI) && overlayUI != null)
        {
            return overlayUI as T;
        }

        // 이미 참조된 TutorialDialogueUI가 있으면 그대로 반환
        if (typeof(T) == typeof(TutorialDialogueUI) && tutorialUI != null)
        {
            return tutorialUI as T;
        }

        // Resources 폴더에서 UI 프리팹을 불러옴
        GameObject prefab = Resources.Load<GameObject>(resourcePath);

        // UIRoot라는 이름을 가진 UI 최상단 부모 오브젝트 찾기
        Transform uiRoot = GameObject.Find("UIRoot")?.transform;

        // 프리팹과 UIRoot가 모두 존재할 때만 인스턴스를 생성
        if (prefab != null && uiRoot != null)
        {
            // UIRoot의 자식으로 UI 프리팹을 생성
            GameObject instance = Instantiate(prefab, uiRoot);
            T          ui       = instance.GetComponent<T>();

            // 기본적으로 비활성화 상태로 시작
            ui.gameObject.SetActive(false);

            // 생성한 UI를 참조 저장 (캐싱)
            if (typeof(T) == typeof(OverlayDialogueUI))
            {
                overlayUI = ui as OverlayDialogueUI;
            }

            if (typeof(T) == typeof(TutorialDialogueUI))
            {
                tutorialUI = ui as TutorialDialogueUI;
            }

            return ui;
        }

        // 생성 실패 시 오류 출력
        Debug.LogError($"[DialogueController] {typeof(T).Name} 생성 실패");
        return null;
    }
}

// 리소스 로더
public static class DialogueResourceLoader
{
    public static Sprite LoadPortrait(string key)
    {
        return Resources.Load<Sprite>($"Portraits/{key}");
    }

    public static Sprite LoadBackground(string key)
    {
        return Resources.Load<Sprite>($"Backgrounds/{key}");
    }
}