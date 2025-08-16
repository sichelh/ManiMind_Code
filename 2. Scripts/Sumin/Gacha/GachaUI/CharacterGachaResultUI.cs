using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CharacterGachaResultUI : UIBase
{
    [SerializeField] private Button resultExitBtn;
    [SerializeField] private CharacterGachaSlotUI[] slots;
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

    public void ShowCharacters(PlayerUnitSO[] characters)
    {
        resultSeq.OnComplete(() =>
        {
            for (int i = 0; i < characters.Length; i++)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Initialize(characters[i]);

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
