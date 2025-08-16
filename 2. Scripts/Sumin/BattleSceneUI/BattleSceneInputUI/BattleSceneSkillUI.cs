using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSceneSkillUI : UIBase
{
    [SerializeField] private List<BattleSceneSkillSlot> skillSlot;
    [SerializeField] private BattleSceneAttackSlot attackSlot;
    [SerializeField] private RectTransform rect;
    [SerializeField] private CanvasGroup canvasGroup;

    private Sequence seq;
    private Vector2 originalPos;
    private bool initialized = false;

    // Skill 선택 Exit 버튼
    public void OnClickSkillExit()
    {
        InputManager.Instance.OnClickSkillExitButton();
    }

    public void OnEnable()
    {
        // 초기 위치 저장
        if (!initialized)
        {
            originalPos = rect.anchoredPosition;
            initialized = true;
        }
    }

    public override void Open()
    {
        base.Open();

        canvasGroup.DOKill();
        rect.DOKill();
        seq.Kill();

        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
        rect.DOAnchorPos(originalPos, 0.3f).From(originalPos + (Vector2.down * 500f)).SetEase(Ease.OutQuint);
    }

    public override void Close()
    {
        seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(0f, 0.6f).SetEase(Ease.InOutSine));
        seq.Join(rect.DOAnchorPos(originalPos + (Vector2.down * 500f), 0.6f).From(originalPos).SetEase(Ease.OutQuint));
        seq.OnComplete(() => base.Close());
    }

    //유닛이 보유한 스킬 리스트들을 차례로 슬롯에 넣어주기
    public void UpdateSkillList(Unit selectedUnit)
    {
        UIManager.Instance.Open(this, false);
        if (selectedUnit is PlayerUnitController playerUnit)
        {
            for (int i = 0; i < skillSlot.Count; i++)
            {
                skillSlot[i].Initialize(playerUnit.SkillController.skills[i], i);
            }
        }
    }

    // 스킬 선택중 표시
    public void ToggleHighlightSkillSlot(bool toggle, int index)
    {
        skillSlot[index].ToggleHighlightSkillBtn(toggle);
    }

    public void ToggleHighlightBasicAttack(bool toggle)
    {
        attackSlot.ToggleHighlightAttackBtn(toggle);
    }
}