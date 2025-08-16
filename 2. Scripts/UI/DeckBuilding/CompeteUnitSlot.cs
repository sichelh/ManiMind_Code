using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CompeteUnitSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int index;

    private EntryDeckData competeUnitData;
    private UIDeckBuilding UIDeckBuilding => UIManager.Instance.GetUIComponent<UIDeckBuilding>();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (competeUnitData != null &&
            (TutorialManager.Instance == null || !TutorialManager.Instance.IsActive))
        {
            UIDeckBuilding.RemoveUnitInDeck(competeUnitData);
            competeUnitData = null;
        }
    }

    public void SetCompeteUnitData(EntryDeckData competeUnitData)
    {
        this.competeUnitData = competeUnitData;
    }
}