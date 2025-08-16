public interface ISelectable
{
    // 클릭 가능한 오브젝트
    Unit SelectedUnit { get; }
    void ToggleSelectableIndicator(bool toggle);
    void ToggleSelectedIndicator(bool toggle);
    void PlaySelectEffect();
}
