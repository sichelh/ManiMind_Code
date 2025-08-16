using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoldPressDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float requireHoldTime = 0.5f;
    private float holdTimer = 0f;
    private bool isHolding = false;

    public event Action OnLongPress;

    public void OnPointerDown(PointerEventData evnetData)
    {
        isHolding = true;
        holdTimer = 0f; 
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
    }

    void Update()
    {
        if (!isHolding) return;

        holdTimer += Time.unscaledDeltaTime;
        if (holdTimer >= requireHoldTime)
        {
            isHolding = false;
            OnLongPress?.Invoke();
        }
    }
}
