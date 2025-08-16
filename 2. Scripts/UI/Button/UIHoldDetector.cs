using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoldDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float holdTime;
    [SerializeField] private Image holdProgressImage;

    public event Action OnHoldTriggered;


    private Coroutine holdCoroutine;
    private bool isHolding = false;
    public bool WasHoldTriggered { get; set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (holdCoroutine == null && OnHoldTriggered != null)
        {
            holdCoroutine = StartCoroutine(HoldCoroutine());
        }
    }

    private IEnumerator HoldCoroutine()
    {
        isHolding = true;
        float elapsed = 0f;

        if (holdProgressImage != null)
        {
            holdProgressImage.fillAmount = 0f;
        }

        while (elapsed < holdTime)
        {
            elapsed += Time.deltaTime;
            if (holdProgressImage != null)
            {
                if (elapsed > 0.1f && !holdProgressImage.gameObject.activeSelf)
                {
                    holdProgressImage.gameObject.SetActive(true);
                }

                holdProgressImage.fillAmount = Mathf.Clamp01(elapsed / holdTime);
            }

            yield return null;
        }

        if (isHolding)
        {
            WasHoldTriggered = true;
            OnHoldTriggered?.Invoke();
        }

        if (holdProgressImage != null)
        {
            holdProgressImage.gameObject.SetActive(false);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("OnPointer Up");
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        isHolding = false;
        if (holdProgressImage != null)
        {
            holdProgressImage.gameObject.SetActive(false);
        }
    }
}