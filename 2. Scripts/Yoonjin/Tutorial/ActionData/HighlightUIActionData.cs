using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "HighlightUIAction", menuName = "ScriptableObjects/Tutorial/Actions/HighlightUI")]
public class HighlightUIActionData : TutorialActionData
{
    public bool requireDoubleClick = false; // 더블 클릭 필요 여부
    public bool requireHold = false;        // 홀드 버튼 여부
    public string targetButtonName;         // 강조할 버튼
    public bool autoBlockOthers = true;     // 나머지 버튼 자동 차단 여부

    public override TutorialActionType ActionType { get; } = TutorialActionType.HighlightUI;
}