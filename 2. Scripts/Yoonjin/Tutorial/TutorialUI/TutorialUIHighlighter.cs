using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TutorialUIHighlighter
{
    private static GameObject highlightEffect;
    private static GameObject holdEffect;

    // 대상 버튼 위에 하이라이트 이펙트를 덮음
    public static void Highlight(GameObject target, bool requireLongPress)
    {
        if (highlightEffect == null)
        {
            // Resources 폴더에서 하이라이트 프리팹 로드 및 인스턴스화
            highlightEffect = GameObject.Instantiate(Resources.Load<GameObject>("UI/HighlightEffect"));
        }

        if (holdEffect == null)
        {
            holdEffect = GameObject.Instantiate(Resources.Load<GameObject>("UI/HoldEffect"));
        }

        var effectRect = highlightEffect.GetComponent<RectTransform>();
        var targetRect = target.GetComponent<RectTransform>();
        if (effectRect == null || targetRect == null)
        {
            Debug.LogError("RectTransform이 누락된 오브젝트가 있습니다.");
            return;
        }

        // 버튼 내부에 자식으로 붙이고
        highlightEffect.transform.SetParent(target.transform, false);

        // 버튼 크기와 위치에 딱 맞게 채우기
        effectRect.anchorMin = Vector2.zero;
        effectRect.anchorMax = Vector2.one;
        effectRect.offsetMin = Vector2.zero;
        effectRect.offsetMax = Vector2.zero;
        effectRect.localScale = Vector3.one;

        // 버튼 자식 중 가장 위에 위치
        highlightEffect.transform.SetAsLastSibling();
        highlightEffect.SetActive(true);

        if (holdEffect != null)
        {
            holdEffect.transform.SetParent(highlightEffect.transform, false);

            holdEffect.transform.SetAsLastSibling();
            holdEffect.SetActive(requireLongPress);
        }
    }

    // 하이라이트 효과를 숨김
    public static void Clear()
    {
        if (holdEffect != null) holdEffect.SetActive(false);
        if (highlightEffect != null) highlightEffect.SetActive(false);
    }
}
