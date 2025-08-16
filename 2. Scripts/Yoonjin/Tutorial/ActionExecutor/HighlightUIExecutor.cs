using UnityEngine;
using UnityEngine.UI;

public class HighlightUIExecutor : TutorialActionExecutor
{
    private Button targetButton;
    private UIHoldDetector holdDetector;

    private bool requireDoubleClick;
    private int clickCount = 0;
    private bool requireLongPress;


    public override void Enter(TutorialActionData actionData)
    {
        HighlightUIActionData data = actionData as HighlightUIActionData;
        if (data == null)
        {
            return;
        }

        requireDoubleClick = data.requireDoubleClick;
        clickCount = 0;
        requireLongPress = data.requireHold;

        // 대상 버튼 찾기
        targetButton = GameObject.Find(data.targetButtonName)?.GetComponent<Button>();
        if (targetButton == null)
        {
            Debug.LogError($"[튜토리얼] '{data.targetButtonName}' 버튼을 찾을 수 없습니다.");
            return;
        }

        //  홀드 감지 컴포넌트 부착 또는 가져오기
        holdDetector = targetButton.GetComponent<UIHoldDetector>();
        if (holdDetector != null)
        {
            // holdDetector = targetButton.gameObject.AddComponent<UIHoldDetector>();
            holdDetector.OnHoldTriggered += OnHoldTriggered;
        }


        if (requireLongPress)
        {
            // 홀드 요구 시 클릭 비활성화
            targetButton.interactable = false;
        }
        else
        {
            // 클릭으로 튜토리얼 진행
            targetButton.onClick.AddListener(OnTargetButtonClicked);
        }

        // 주변 클릭 차단
        if (data.autoBlockOthers)
        {
            TutorialUIBlocker.BlockAllExcept(targetButton.gameObject);
        }

        // 버튼 강조
        TutorialUIHighlighter.Highlight(targetButton.gameObject, requireLongPress);
    }

    // 일반 클릭 처리
    private void OnTargetButtonClicked()
    {
        clickCount++;

        if (holdDetector != null && holdDetector.WasHoldTriggered)
        {
            Debug.Log("[튜토리얼] 홀드된 상태에서 클릭 무시");
            holdDetector.WasHoldTriggered = false;
            return;
        }

        if (requireDoubleClick && clickCount < 2)
        {
            return;
        }

        manager.CompleteCurrentStep();
    }

    // 홀드 완료 시 처리
    private void OnHoldTriggered()
    {
        if (!requireLongPress)
        {
            // 클릭 스텝에서는 홀드는 무시
            Debug.Log("[튜토리얼] 홀드 발생했지만 클릭만 요구됨 → 무시");
            return;
        }

        if (holdDetector != null && holdDetector.WasHoldTriggered)
        {
            holdDetector.WasHoldTriggered = false;
        }

        // 홀드 성공 시 버튼 다시 활성화하고 다음 스텝으로
        targetButton.interactable = true;
        manager.CompleteCurrentStep();
    }

    public override void Exit()
    {
        // 이벤트 리스너 정리
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(OnTargetButtonClicked);
            if (holdDetector != null)
            {
                holdDetector.OnHoldTriggered -= OnHoldTriggered;
            }
        }

        // 하이라이트 및 블로커 제거
        TutorialUIBlocker.Clear();
        TutorialUIHighlighter.Clear();
    }
}