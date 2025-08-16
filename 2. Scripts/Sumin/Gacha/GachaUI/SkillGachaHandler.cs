public class SkillGachaHandler : IGachaHandler
{
    private readonly SkillGachaSystem gachaSystem;
    private readonly SkillGachaResultUI resultUI;
    private readonly UIManager uiManager;
    private readonly AccountManager accountManager;

    public SkillGachaHandler(SkillGachaSystem skillGachaSystem, SkillGachaResultUI skillResultUI)
    {
        this.gachaSystem = skillGachaSystem;
        this.resultUI = skillResultUI;
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
        return "스킬 소환";
    }

    public void DrawAndDisplayResult(int count)
    {
        AudioManager.Instance.PlaySFX(SFXName.GachaCacheSound.ToString());
        int cost = GetTotalCost(count);
        accountManager.UseOpal(cost);

        GachaResult<ActiveSkillSO>[] skills = gachaSystem.DrawSkills(count);

        uiManager.Open(resultUI);
        resultUI.ShowSkills(skills);
    }

    public int GetTotalCost(int count)
    {
        if (count == 10)
            return gachaSystem.DrawCost * 9;
        return gachaSystem.DrawCost * count;
    }
}
