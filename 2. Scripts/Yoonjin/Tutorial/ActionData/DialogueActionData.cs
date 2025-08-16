using UnityEngine;

[CreateAssetMenu(fileName = "DialogueAction", menuName = "ScriptableObjects/Tutorial/Actions/Dialogue", order = 0)]
public class DialogueActionData : TutorialActionData
{
    [TextArea] public string dialogGroupKey;

    public override TutorialActionType ActionType { get; } = TutorialActionType.Dialogue;
}