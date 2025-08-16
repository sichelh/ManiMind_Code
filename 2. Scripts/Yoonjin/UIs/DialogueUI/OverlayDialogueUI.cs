using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OverlayDialogueUI : MonoBehaviour, IDialogueUI
{
    [Header("대사창 구성")]
    [SerializeField] private TMP_Text nameText; // 캐릭터 이름

    [SerializeField] private TMP_Text dialogueText;    // 대사 텍스트
    [SerializeField] private Image leftPortraitImage;  // 초상화 왼쪽 이미지
    [SerializeField] private Image rightPortraitImage; // 초상화 오른쪽 이미지
    [SerializeField] private CanvasGroup leftPortraitGroup;
    [SerializeField] private CanvasGroup rightPortraitGroup;

    [Header("타자 효과")]
    [SerializeField] private TypeWriter typeWriter; // 인스펙터에서 dialogueText 할당

    public bool IsTyping => typeWriter != null && typeWriter.IsTyping;

    public void Show(DialogueLine line, bool withTyping = true)
    {
        nameText.text = line.characterName;
        dialogueText.text = line.dialogue;

        // 초상화 불러오기
        Sprite leftSprite  = DialogueResourceLoader.LoadPortrait(line.portraitLeft);
        Sprite rightSprite = DialogueResourceLoader.LoadPortrait(line.portraitRight);

        leftPortraitImage.sprite = leftSprite;
        rightPortraitImage.sprite = rightSprite;

        leftPortraitImage.gameObject.SetActive(leftSprite != null);
        rightPortraitImage.gameObject.SetActive(rightSprite != null);

        // 말하는 캐릭터가 왼쪽인지 오른쪽인지 판별
        bool isLeftSpeaking  = line.portraitLeft == line.portraitKey;
        bool isRightSpeaking = line.portraitRight == line.portraitKey;

        // 밝기 조절
        leftPortraitImage.DOColor(isLeftSpeaking ? Color.white : new Color(0.3f, 0.3f, 0.3f), 0.25f);
        rightPortraitImage.DOColor(isRightSpeaking ? Color.white : new Color(0.3f, 0.3f, 0.3f), 0.25f);

        // 텍스트
        if (withTyping && typeWriter != null) typeWriter.StartTyping(line.dialogue);
        else
        {
            dialogueText.text = line.dialogue;
            dialogueText.ForceMeshUpdate();
            dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        }
        gameObject.SetActive(true);
    }

    public void CompleteTyping()
    {
        if (typeWriter != null) typeWriter.CompleteTyping();
    }

    public void OnClickNext()
    {
        AudioManager.Instance.PlaySFX(SFXName.SwipeDialogueSound.ToString());
        if (DialogueController.Instance.TryCompleteTyping()) return;
        DialogueController.Instance.Next();
    }

    public void Skip()
    {
        AudioManager.Instance.PlaySFX(SFXName.SwipeDialogueSound.ToString());
        DialogueController.Instance.EndDialogue();
    }
}