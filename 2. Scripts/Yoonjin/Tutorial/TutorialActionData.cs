using UnityEngine;

public abstract class TutorialActionData : ScriptableObject
{
    public abstract TutorialActionType ActionType { get; }
}