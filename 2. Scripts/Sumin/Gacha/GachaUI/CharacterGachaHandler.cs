public class CharacterGachaHandler : IGachaHandler
{
    private readonly CharacterGachaSystem gachaSystem;
    private readonly CharacterGachaResultUI resultUI;
    private readonly UIManager uiManager;
    private readonly AccountManager accountManager;

    public CharacterGachaHandler(CharacterGachaSystem characterGachaSystem, CharacterGachaResultUI characterResultUI)
    {
        this.gachaSystem = characterGachaSystem;
        this.resultUI = characterResultUI;
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
        return "영웅 소환";
    }

    public void DrawAndDisplayResult(int count)
    {
        AudioManager.Instance.PlaySFX(SFXName.GachaCacheSound.ToString());
        int cost = GetTotalCost(count);
        accountManager.UseOpal(cost);

        PlayerUnitSO[] characters = gachaSystem.DrawCharacters(count);

        uiManager.Open(resultUI);
        resultUI.ShowCharacters(characters);
    }

    public int GetTotalCost(int count)
    {
        if (count == 10)
            return gachaSystem.DrawCost * 9;
        return gachaSystem.DrawCost * count;
    }
}
