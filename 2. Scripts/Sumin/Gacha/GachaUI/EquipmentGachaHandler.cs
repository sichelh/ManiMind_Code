public class EquipmentGachaHandler : IGachaHandler
{
    private readonly EquipmentGachaSystem gachaSystem;
    private readonly EquipmentGachaResultUI resultUI;
    private readonly UIManager uiManager;
    private readonly AccountManager accountManager;

    public EquipmentGachaHandler(EquipmentGachaSystem equipmentGachaSystem, EquipmentGachaResultUI equipmentResultUI)
    {
        this.gachaSystem = equipmentGachaSystem;
        this.resultUI = equipmentResultUI;
        uiManager = UIManager.Instance;
        accountManager = AccountManager.Instance;
    }

    public bool CanDraw(int count)
    {
        int totalCost = GetTotalCost(count);
        return accountManager.CanUseOpal(totalCost);
    }

    public string GetGachaTypeName()
    {
        return "장비 소환";
    }

    public void DrawAndDisplayResult(int count)
    {
        AudioManager.Instance.PlaySFX(SFXName.GachaCacheSound.ToString());
        int cost = GetTotalCost(count);
        accountManager.UseOpal(cost);

        EquipmentItemSO[] equipments = gachaSystem.DrawEquipments(count);

        uiManager.Open(uiManager.GetUIComponent<EquipmentGachaResultUI>());
        resultUI.ShowEquipments(equipments);
    }

    public int GetTotalCost(int count)
    {
        if (count == 10)
            return gachaSystem.DrawCost * 9;
        return gachaSystem.DrawCost * count;
    }
}
