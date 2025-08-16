using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillForDetailButton : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] private GameObject skillDetailPopup;
    [SerializeField] private float hideDelay;

    private bool interactable = true;
    private Coroutine hideCoroutine;

    private bool hasHeldLongEnough = false;
    public bool IsClickBolcked => hasHeldLongEnough;

    private void Awake()
    {
        UIHoldDetector holdDetector = GetComponent<UIHoldDetector>();
        if (holdDetector != null)
        {
            holdDetector.OnHoldTriggered += () =>
            {
                if (interactable)
                {
                    hasHeldLongEnough = true;
                    skillDetailPopup.SetActive(true);
                }
            };
        }
    }

    // 스킬일 때만 Interact하도록 설정
    public void SetInteractable(bool value)
    {
        interactable = value;
    }

    private IEnumerator HidePopupAfterDelay() // 뗐을 때 일정 시간 후 팝업 끄기
    {
        yield return new WaitForSeconds(hideDelay);
        skillDetailPopup.gameObject.SetActive(false);
        hasHeldLongEnough = false;
    }

    // 버튼 꺼졌을때 자동으로 꺼지게
    private void OnDisable()
    {
        if (skillDetailPopup != null && skillDetailPopup.activeSelf)
        {
            skillDetailPopup.SetActive(false);
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable)
        {
            return;
        }

        StartCoroutine(HidePopupAfterDelay());
    }
}