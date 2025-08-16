using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class TutorialUIBlocker
{
    private static List<Selectable> previouslyEnabled = new();

    // 지정된 버튼만 클릭 가능하고 나머지는 막음
    public static void BlockAllExcept(GameObject allowed, List<GameObject> exceptions = null)
    {
        previouslyEnabled.Clear();

        var allSelectables = GameObject.FindObjectsOfType<Selectable>();

        foreach (var sel in allSelectables)
        {
            if (sel == null) continue;

            bool allow =
                sel.gameObject == allowed ||
                (exceptions != null && exceptions.Contains(sel.gameObject));

            if (!allow && sel.interactable)
            {
                sel.interactable = false;
                previouslyEnabled.Add(sel);
            }
        }
    }

    // 전체 UI 인터랙션 차단
    public static void BlockAll()
    {
        previouslyEnabled.Clear();

        var allSelectables = GameObject.FindObjectsOfType<Selectable>();

        foreach (var sel in allSelectables)
        {
            if (sel != null && sel.interactable)
            {
                sel.interactable = false;
                previouslyEnabled.Add(sel);
            }
        }
    }

    // 차단된 UI 상호작용을 모두 복원
    public static void Clear()
    {
        foreach (var sel in previouslyEnabled)
        {
            if (sel != null)
                sel.interactable = true;
        }

        previouslyEnabled.Clear();
    }
}
