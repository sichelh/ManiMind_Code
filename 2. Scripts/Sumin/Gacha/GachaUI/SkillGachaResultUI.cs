using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SkillGachaResultUI : UIBase
{
    [SerializeField] private Button resultExitBtn;
    [SerializeField] private SkillGachaSlotUI[] slots;
    private CanvasGroup canvasGroup;
    private Sequence resultSeq;

    void Start()
    {
        resultExitBtn.onClick.RemoveAllListeners();
        resultExitBtn.onClick.AddListener(() => OnResultExitBtn());
        ResetSlots();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public override void Open()
    {
        base.Open();

        canvasGroup.DOKill();
        canvasGroup.alpha = 0f;

        resultSeq = DOTween.Sequence();
        resultSeq.Append(canvasGroup.DOFade(1f, 0.5f));
    }

    // 스킬 뽑기 결과 업데이트
    public void ShowSkills(GachaResult<ActiveSkillSO>[] skills)
    {
        resultSeq.OnComplete(() =>
        {
            for (int i = 0; i < skills.Length; i++)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Initialize(skills[i].GachaReward);

                if (skills[i].IsDuplicate) // 중복이면 티어 대신 보상금 띄워주기
                {
                    slots[i].ShowCompensation(skills[i].CompensationAmount);
                }

                slots[i].transform.localScale = Vector3.zero;
                float delay = i * 0.1f;
                slots[i].transform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(delay);
                DOVirtual.DelayedCall(delay, () =>
                {
                    AudioManager.Instance.PlaySFX(SFXName.GachaCardSound.ToString()); // 여기에 원하는 효과음 함수 호출
                });
            }
        });
    }

    public override void Close()
    {
        ResetSlots();
        base.Close();
    }

    public void OnResultExitBtn()
    {
        UIManager.Instance.Close(this);
    }

    private void ResetSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].gameObject.SetActive(false);
        }
    }
}
