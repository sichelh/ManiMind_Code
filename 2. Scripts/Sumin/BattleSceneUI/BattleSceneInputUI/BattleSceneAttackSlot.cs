using UnityEngine;

public class BattleSceneAttackSlot : MonoBehaviour
{
    [SerializeField] private GameObject highLight;

    // 기본공격 버튼 클릭하면 전달
    public void OnClickBasicAttack()
    {
        AudioManager.Instance.PlaySFX(SFXName.OpenUISound.ToString());
        InputManager.Instance.SelectBasicAttack();
        ToggleHighlightAttackBtn(true);
    }

    public void ToggleHighlightAttackBtn(bool toggle)
    {
        highLight.SetActive(toggle);
    }
}
