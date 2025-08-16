using UnityEngine;

public class BattleSceneUnitIndicator : MonoBehaviour
{
    public GameObject SelectableIndicator; // 선택 가능한 유닛 표시
    public ParticleSystem SelectEffect; // 유닛 선택 시 표시
    public GameObject SelectedIndicator; // 선택중인 유닛 표시

    // 유닛 선택 가능 토글
    public void ToggleSelectableIndicator(bool toggle)
    {
        if (SelectableIndicator == null)
            Debug.LogError("SelectableIndicator Prefab을 연결해주세요.");
        SelectableIndicator.SetActive(toggle);
    }

    // 유닛 선택됨 표시 토글
    public void ToggleSelectedIndicator(bool toggle)
    {
        if (SelectedIndicator == null)
            Debug.LogError("SelectedIndicator Prefab을 연결해주세요.");
        SelectedIndicator.SetActive(toggle);
    }

    // 유닛 선택 파티클 재생
    public void PlaySelectEffect()
    {
        if (SelectEffect == null)
            Debug.LogError("SelectEffect Prefab을 연결해주세요.");
        SelectEffect.Play();
    }
}
