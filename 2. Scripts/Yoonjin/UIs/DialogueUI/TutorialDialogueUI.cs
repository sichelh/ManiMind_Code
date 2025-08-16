using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDialogueUI : MonoBehaviour, IDialogueUI
{
    [Header("대사창 구성")]
    [SerializeField] private TMP_Text dialogueText; // 대사 텍스트

    [Header("타자 효과")]
    [SerializeField] private TypeWriter typeWriter; // 인스펙터에서 dialogueText 할당

    public bool IsTyping => typeWriter != null && typeWriter.IsTyping;

    public void Show(DialogueLine line, bool withTyping = true)
    {
        if (withTyping && typeWriter != null)
        {
            typeWriter.StartTyping(line.dialogue);
        }

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
        if (DialogueController.Instance.TryCompleteTyping()) return;
        DialogueController.Instance.Next();
    }
}