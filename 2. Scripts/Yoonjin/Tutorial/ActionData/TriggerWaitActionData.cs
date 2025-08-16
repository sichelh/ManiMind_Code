using UnityEngine;

public enum TriggerType
{
    SceneLoaded,
    MonsterKilled,
    BattleVictory,
    CustomEvent
}

[CreateAssetMenu(fileName = "TriggerWaitAction", menuName = "ScriptableObjects/Tutorial/Actions/TriggerWait", order = 0)]
public class TriggerWaitActionData : TutorialActionData
{
    public TriggerType triggerType;
    public string triggerEventName;
    public bool blockAllUI;
    public override TutorialActionType ActionType { get; } = TutorialActionType.TriggerWait;
}