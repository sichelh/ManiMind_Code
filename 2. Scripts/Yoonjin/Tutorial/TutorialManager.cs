using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    public enum TutorialPhase
    {
        DeckBuildingBefore = 0,
        DeckBuildingAfter = 1,
        LevelUp = 2
    }

    [SerializeField] private TutorialTable tutorialTable;

    // 행동별 실행기 매핑 (FSM처럼 동작)
    private Dictionary<TutorialActionType, TutorialActionExecutor> executorMap;

    private TutorialStepSO currentStep;
    public TutorialStepSO CurrentStep => currentStep;

    [HideInInspector]
    public bool IsActive;

    // 현재 대기 중인 액션 개수
    private int waitingActionCount;

    protected override void Awake()
    {
        base.Awake();

        // 각 행동에 대한 실행기 등록
        executorMap = new Dictionary<TutorialActionType, TutorialActionExecutor>
        {
            { TutorialActionType.Dialogue, new DialogueActionExecutor() },
            { TutorialActionType.HighlightUI, new HighlightUIExecutor() },
            { TutorialActionType.TriggerWait, new TriggerWaitExecutor() },
            { TutorialActionType.Reward, new RewardActionExecutor() },
            { TutorialActionType.ImagePopup, new ImagePopupActionExecutor() }
        };

        // 실행기에 튜토리얼 매니저 주입
        foreach (TutorialActionExecutor exec in executorMap.Values)
        {
            exec.SetManager(this);
        }

        // 테이블 가져오기 
        tutorialTable = TableManager.Instance.GetTable<TutorialTable>();
    }


    private void Start()
    {
        SaveTutorialData tutorialData = SaveLoadManager.Instance
            .SaveDataMap.GetValueOrDefault(SaveModule.Tutorial) as SaveTutorialData;

        // 데이터가 없으면 새로 생성
        if (tutorialData == null)
        {
            tutorialData = new SaveTutorialData { Phase = TutorialPhase.DeckBuildingBefore, IsCompleted = false };

            SaveLoadManager.Instance.SaveDataMap[SaveModule.Tutorial] = tutorialData;
            SaveLoadManager.Instance.SaveModuleData(SaveModule.Tutorial);
        }

        // 튜토리얼 완료된 경우 비활성화
        if (tutorialData.IsCompleted)
        {
            IsActive = false;
            return;
        }

        IsActive = true;

        // DeckBuildingBefore 페이즈에서는 장비, 스킬, 유닛 초기화
        if (tutorialData.Phase == TutorialPhase.DeckBuildingBefore)
        {
            List<EntryDeckData> unitList = AccountManager.Instance.GetPlayerUnits();

            foreach (EntryDeckData unit in unitList)
            {
                // 출전 중인 유닛 해제
                // unit.Compete(-1, false);

                // 장비 해제
                List<EquipmentType> equippedTypes = unit.EquippedItems.Keys.ToList();
                foreach (EquipmentType type in equippedTypes)
                {
                    unit.UnEquipItem(type);
                }

                // 스킬 해제
                foreach (SkillData skill in unit.SkillDatas)
                {
                    if (skill != null)
                    {
                        unit.UnEquipSkill(skill);
                    }
                }

                DeckSelectManager.Instance.RemoveUnitInDeck(unit);
            }

            SaveLoadManager.Instance.SaveModuleData(SaveModule.InventoryUnit);
        }

        // 재시작 지점 설정
        int resumeStep = tutorialTable.DataDic.Values
            .Where(step => step.phase == tutorialData.Phase && step.isResumeEntryPoint)
            .Select(step => step.ID)
            .LastOrDefault();

        // 유효한 ID인지 확인 (예외 처리 추가)

        TutorialStepSO tableData = tutorialTable.GetDataByID(resumeStep);

        if (tableData != null)
        {
            if (tutorialData.Phase == TutorialPhase.DeckBuildingBefore)
            {
                Debug.Log("[튜토리얼] 기본 ID 0으로 시작합니다.");
                resumeStep = 0;
            }
        }
        else
        {
            Debug.Log($"[튜토리얼] resumeStep ID {resumeStep}이 존재하지 않습니다. 튜토리얼을 종료합니다.");
            EndTutorial();
            return;
        }

        GoToStep(resumeStep);
        Debug.Log($"resumeStep {resumeStep}부터 재시작합니다!");
    }


    public void GoToStep(int id)
    {
        Debug.Log($"[튜토리얼] GoToStep 호출됨 (ID: {id})");

        if (!tutorialTable.DataDic.TryGetValue(id, out currentStep))
        {
            Debug.Log($"[튜토리얼] Step ID {id}를 찾을 수 없습니다. 튜토리얼 종료.");
            EndTutorial();
            return;
        }

        // 현재 페이즈 저장
        if (SaveLoadManager.Instance.SaveDataMap.GetValueOrDefault(SaveModule.Tutorial) is SaveTutorialData tutorialData)
        {
            tutorialData.Phase = currentStep.phase;
            SaveLoadManager.Instance.SaveModuleData(SaveModule.Tutorial);
        }

        if (currentStep.Actions == null || currentStep.Actions.Count == 0)
        {
            Debug.Log($"[튜토리얼] Step {id}에 등록된 Actions가 없습니다!");
            CompleteCurrentStep(); // 다음으로 넘겨도 무방
            return;
        }

        waitingActionCount = currentStep.Actions.Count;
        Debug.Log($"[튜토리얼] Step {id}에서 {waitingActionCount}개의 액션 실행 시작");

        foreach (TutorialActionData action in currentStep.Actions)
        {
            if (action == null)
            {
                Debug.LogWarning($"[튜토리얼] Null 액션이 포함되어 있습니다.");
                NotifyActionComplete(); // 무시하고 완료 카운트 감소
                continue;
            }

            if (executorMap.TryGetValue(action.ActionType, out TutorialActionExecutor executor))
            {
                executor.Enter(action);
            }
            else
            {
                Debug.Log($"[튜토리얼] ActionType({action.ActionType})에 대한 실행기를 찾을 수 없습니다.");
                NotifyActionComplete(); // 실행 실패시에도 진행
            }
        }
    }

    public void NotifyActionComplete()
    {
        waitingActionCount--;

        Debug.Log($"[튜토리얼] 실행 완료됨. 남은 액션 수: {waitingActionCount}");

        if (waitingActionCount <= 0)
        {
            CompleteCurrentStep();
        }
    }


    // 현재 스텝을 종료하고 다음 스텝으로 전환
    public void CompleteCurrentStep()
    {
        // 모든 실행기에 대해 Exit 호출
        foreach (TutorialActionData action in currentStep.Actions)
        {
            if (action == null)
            {
                continue;
            }

            if (executorMap.TryGetValue(action.ActionType, out TutorialActionExecutor executor))
            {
                executor.Exit();
            }
        }

        GoToStep(currentStep.NextID);
    }

    // 튜토리얼 종료
    public void EndTutorial()
    {
        IsActive = false;

        if (SaveLoadManager.Instance.SaveDataMap.GetValueOrDefault(SaveModule.Tutorial) is SaveTutorialData tutorialData)
        {
            tutorialData.IsCompleted = true;

            SaveLoadManager.Instance.SaveModuleData(SaveModule.Tutorial);
        }

        //치트
        AccountManager.Instance.Cheat();
        Debug.Log("튜토리얼 종료!");
    }
}