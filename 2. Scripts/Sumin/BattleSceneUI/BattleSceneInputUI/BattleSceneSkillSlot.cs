using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSceneSkillSlot : MonoBehaviour
{
    [Header("스킬 슬롯 하이라이트")]
    [SerializeField] private GameObject highLight;

    [Header("스킬 슬롯 앞면 : 남은 재사용 횟수")]
    [SerializeField] private Button FrontSkillBtn;

    [SerializeField] private Image skillIconImage;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI reuseCountText;
    [SerializeField] private Image skillTierFrameImage;

    [Header("스킬 슬롯 뒷면 : 스킬 코스트(쿨타임)")]
    [SerializeField] private Button BackSkillBtn;

    [SerializeField] private TextMeshProUGUI coolDownText;
    [SerializeField] private GameObject lockImage;

    [Header("뒷면 버튼 : 뒤집는 속도와 뒤집고 있는 시간")]
    [SerializeField] private float flipDuration;

    [SerializeField] private float waitFlippedTime;

    [Header("스킬 팝업")]
    [SerializeField] private SkillDetailPopupUI skillDetailPopup;

    [SerializeField] private SkillForDetailButton skillForDetailButton;

    [SerializeField] private List<Sprite> skillTierSprites;

    // 스킬 데이터들
    private SkillData selectedSkillData;
    private int currentskillIndex;
    private int coolDown;
    private int reuseCount;

    private bool isFront = false;

    public void Initialize(SkillData skillData, int index)
    {
        UnLockSkill();

        // 슬롯 번호 기반 이름 부여
        FrontSkillBtn.gameObject.name = $"FrontSkillBtn_{index}";
        BackSkillBtn.gameObject.name = $"BackSkillBtn_{index}";

        // skill data가 없으면 뒤집고 선택불가
        if (skillData == null)
        {
            ToggleSkillSlot(false);
            LockSkill();
            return;
        }

        ToggleSkillSlot(skillData.CheckCanUseSkill()); // 사용 가능 여부에 따라 앞or뒤 켜고 끄기

        // skill data를 넣기
        selectedSkillData = skillData;
        currentskillIndex = index;
        coolDown = skillData.coolDown;
        reuseCount = skillData.reuseCount;

        // UI에 반영
        coolDownText.text = $"{coolDown}";
        reuseCountText.text = $"{reuseCount}";
        skillIconImage.sprite = skillData.skillSo.SkillIcon;
        skillName.text = skillData.skillSo.skillName;
        skillTierFrameImage.sprite = skillTierSprites[(int)skillData.skillSo.activeSkillTier];

        skillDetailPopup.Initialize(skillData);
        skillForDetailButton.SetInteractable(true);


        if (reuseCount <= 0)
        {
            LockSkill();
        }
    }

    // 스킬 버튼 누르면 이 스킬의 슬롯 인덱스에 맞춰서 반영하여 스킬 선택
    public void OnFrontSkillBtn()
    {
        if (skillForDetailButton != null && skillForDetailButton.IsClickBolcked)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(SFXName.OpenUISound.ToString());
        InputManager.Instance.SelectSkill(currentskillIndex);
        ToggleHighlightSkillBtn(true);
    }

    public void ToggleHighlightSkillBtn(bool toggle)
    {
        highLight.SetActive(toggle);
    }

    // 버튼 뒤쪽이 보이면 클릭 시 잠시 앞면 보여줌
    public void OnBackSkillBtn()
    {
        AudioManager.Instance.PlaySFX(SFXName.CardFlipSound.ToString());
        Sequence flip = DOTween.Sequence();

        // y축으로 90도 먼저 회전
        flip.Append(RotateTo(90));

        // 버튼 교체
        flip.AppendCallback(() => { SetCardState(true); });

        // 완전히 회전
        flip.Append(RotateTo(180));

        // 대기
        flip.AppendInterval(waitFlippedTime);

        // 다시 뒤집기
        flip.Append(RotateTo(90));
        flip.AppendCallback(() => { SetCardState(false); });
        flip.Append(RotateTo(0));
    }

    // 회전 애니메이션 메서드
    private Tween RotateTo(float y)
    {
        return transform.DORotate(new Vector3(0, y, 0), flipDuration).SetEase(Ease.Linear);
    }

    // 카드 상태 전환
    private void SetCardState(bool showFront)
    {
        isFront = showFront;

        FrontSkillBtn.gameObject.SetActive(showFront);
        BackSkillBtn.gameObject.SetActive(!showFront);

        // 카드가 뒤집힌 상태이므로, 앞면을 보이게 하려면 Y=180 보정, 다시 돌아오면 원래대로.
        FrontSkillBtn.transform.localRotation = showFront ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

        // 앞면 잠깐 보일 때 버튼 클릭 불가능하게
        FrontSkillBtn.interactable = !showFront;
    }

    // 스킬 슬롯 앞or뒤 토글
    private void ToggleSkillSlot(bool toggle)
    {
        FrontSkillBtn.gameObject.SetActive(toggle);
        BackSkillBtn.gameObject.SetActive(!toggle);
    }

    // 스킬 슬롯 비활성화
    private void LockSkill()
    {
        ColorBlock colorBlock = BackSkillBtn.colors;
        BackSkillBtn.interactable = false;
        colorBlock.normalColor = new Color(0, 0, 0);
        lockImage.SetActive(true);
        coolDownText.gameObject.SetActive(false);
    }

    // 스킬 슬롯 활성화
    private void UnLockSkill()
    {
        ColorBlock colorBlock = BackSkillBtn.colors;
        BackSkillBtn.interactable = true;
        colorBlock.normalColor = new Color(1, 1, 1);
        lockImage.SetActive(false);
        coolDownText.gameObject.SetActive(true);
    }
}