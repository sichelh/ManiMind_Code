using System.Collections.Generic;
using UnityEngine;

// 하나의 대화 시퀀스를 담는 ScriptableObject
// 한 개의 GroupKey에 해당하는 대사 묶음
[CreateAssetMenu(fileName = "NewDialogueGroup", menuName = "ScriptableObjects/Dialogue/DialogueGroup", order = 0)]
public class DialogueGroupSO : ScriptableObject
{
    [Tooltip("이 대사 시퀀스를 구분하는 키. 예: STAGE_1_BEFORE")]
    public string groupKey;

    [Tooltip("이 대사의 출력 형식")]
    public DialogueMode mode = DialogueMode.Fullscreen;

    [Tooltip("이 그룹에 속한 대사 줄들")]
    public List<DialogueLine> lines = new List<DialogueLine>();
}

// 대사 출력 방식 정의
public enum DialogueMode
{
    Overlay,      // 기존 화면에 간단히 출력
    Fullscreen,   // 전용 대화 연출 UI로 출력
    Tutorial      // 튜토리얼 전용 UI
}
