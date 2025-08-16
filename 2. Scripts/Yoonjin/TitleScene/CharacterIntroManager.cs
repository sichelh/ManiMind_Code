using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterIntroManager : MonoBehaviour
{
    /*
     * 캐릭터 등장 씬 연출 스크립트
     *
     * - Start() : 시작 시 PlaceCharacters 코루틴 실행. 전체 연출의 시작점
     * - IEnumerator PlaceCharacters() : 캐릭터들이 하나씩 등장 위치에서 대열 위치로 달려오는 연출을 처리
     * - void StartWalking() : 등장한 캐릭터들이 걷기 애니메이션과 함께 앞으로 전진
     * - IEnumerator ShowStartButton() : 걷기가 일정 시간 지난 후, 스타트 버튼을 등장시킴
     */

    public GameObject[] characterObjects;  // 씬에 직접 배치한 캐릭터 오브젝트들 (7명)
    public Transform[] formationPositions; // 각 캐릭터가 서야 할 대열 위치 (씬에 빈 오브젝트 7개)
    public Transform walkDirection;        // 걷는 방향을 나타내는 기준 오브젝트 (Z+ 방향으로 설정)
    public GameObject startButton;         // 스타트 버튼 UI (걷기 이후에 등장)
    public GameObject startLOGO;           // 스타트 로고
    public GameObject exitButton;
    public Image overlay; // 시작할 때 화면 연출

    // 캐릭터를 추적할 리스트
    private List<GameObject> spawnedCharacters = new();

    private bool isStartButtonShown = false;

    private void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        QualitySettings.vSyncCount = 0;
        ScalableBufferManager.ResizeBuffers(0.7f, 0.7f);
        Application.targetFrameRate = 45;
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN
        Application.targetFrameRate = -1;
                ScalableBufferManager.ResizeBuffers(1f, 1f);

#else
                ScalableBufferManager.ResizeBuffers(1f, 1f);
        Application.targetFrameRate = 120;
#endif
    }


    private void Start()
    {
        overlay.gameObject.SetActive(true);
        overlay.color = new Color(1f, 1f, 1f, 1f);

        // 페이드 아웃 시작 (점점 어두워짐)
        overlay.DOFade(0f, 1.5f).SetEase(Ease.InOutSine);

        // 동시에 캐릭터 등장 연출 시작
        StartCoroutine(PlaceCharacters());

        AudioManager.Instance.SetVolume(AudioType.Master, PlayerPrefs.GetFloat("MasterVolume", 1));
        AudioManager.Instance.SetVolume(AudioType.BGM, PlayerPrefs.GetFloat("BGMVolume", 1));
        AudioManager.Instance.SetVolume(AudioType.SFX, PlayerPrefs.GetFloat("SFXVolume", 1));
    }

    private void Update()
    {
        if (!isStartButtonShown && Input.GetMouseButtonDown(0))
        {
            isStartButtonShown = true;
            StartCoroutine(ShowStartButton());
        }
    }

    // 씬에 배치된 캐릭터들을 대열 위치로 이동
    private IEnumerator PlaceCharacters()
    {
        for (int i = 0; i < characterObjects.Length; i++)
        {
            GameObject character = characterObjects[i];
            character.SetActive(true);

            Vector3 spawnPos  = character.transform.position;   // 현재 위치
            Vector3 targetPos = formationPositions[i].position; // 대열 목표 위치

            // 캐릭터가 목표 위치를 향해 바라보게 설정 (Y 고정)
            character.transform.LookAt(new Vector3(targetPos.x, character.transform.position.y, targetPos.z));

            Animator anim = character.GetComponent<Animator>();
            anim.SetBool("isRunning", true); // Run 상태 유지

            character.transform.DOMove(targetPos, 3f).SetEase(Ease.Linear).OnComplete(() =>
            {
                anim.SetBool("isRunning", false); // 도착하면 Idle로
            });

            spawnedCharacters.Add(character); // 추적 리스트에 추가

            yield return new WaitForSeconds(1.5f); // 다음 캐릭터까지 대기
        }

        yield return new WaitForSeconds(3.2f); // 모두 등장 후 약간 대기

        StartWalking(); // 걷기 시작
    }

    // 캐릭터들이 앞으로 걷기 시작하는 메서드
    private void StartWalking()
    {
        foreach (GameObject character in spawnedCharacters)
        {
            Animator anim = character.GetComponent<Animator>();
            anim.SetBool("isWalking", true);

            // 걷기 시작 전에 캐릭터가 바라보는 방향을 조정
            // Y값 고정
            Vector3 lookTarget = character.transform.position + walkDirection.forward;
            character.transform.LookAt(new Vector3(lookTarget.x, character.transform.position.y, lookTarget.z));

            // 걷는 목표 위치: 현재 위치 + 걷는 방향으로 100
            Vector3 walkTarget = character.transform.position + (walkDirection.forward * 100f);

            // 속도 기준 걷기
            character.transform.DOMove(walkTarget, 1f)
                .SetSpeedBased(true)   // 거리 기반이 아닌 속도 기반 이동
                .SetEase(Ease.Linear); // 일정한 속도
        }

        StartCoroutine(ShowStartButton()); // 스타트 버튼 등장 시작
    }

    // UI 스타트 버튼을 서서히 등장시키는 코루틴
    private IEnumerator ShowStartButton()
    {
        isStartButtonShown = true;

        yield return new WaitForSeconds(1f); // 걷기 도중 약간 대기
        startButton.SetActive(true);         // 버튼 활성화
        exitButton.SetActive(true);
        startLOGO.SetActive(true);

        // 등장 애니메이션 (스케일 점점 키움)
        startButton.transform.localScale = Vector3.zero;
        startLOGO.transform.localScale = Vector3.zero;
        exitButton.transform.localScale = Vector3.zero;

        startButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        startLOGO.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        exitButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    public void OnClickStart()
    {
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());
        LoadSceneManager.Instance.LoadScene("DeckBuildingScene");
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        // 유니티 에디터에서 실행 중일 때는 Play 모드를 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // 실제 빌드된 애플리케이션에서는 게임 종료
    Application.Quit();
#endif
    }

    public void OnClickBannermenu()
    {
        Debug.Log("Test Mode On");
        GameManager.Instance.isTestMode = true;
    }

    public void OnSettingButtonClick()
    {
        PopupManager.Instance.GetUIComponent<SettingPopup>()?.Open();
    }
}