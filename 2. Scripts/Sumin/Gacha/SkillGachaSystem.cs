using System.Collections.Generic;
using UnityEngine;

public class SkillGachaSystem : MonoBehaviour
{
    [SerializeField] private ActiveSkillTable activeSkillTable;

    private int drawCost = 0;
    public int DrawCost => drawCost;

    private GachaManager<ActiveSkillSO> gachaManager;

    private void Awake()
    {
        gachaManager = new GachaManager<ActiveSkillSO>(new RandoomSkillGachaStrategy());
        drawCost = Define.GachaDrawCosts[GachaType.Skill];
    }

    // 몬스터 스킬 제외
    private List<ActiveSkillSO> GetSkillDatas()
    {
        List<ActiveSkillSO> skills = new();

        for (int i = 0; i < (int)JobType.Monster; i++)
        {
            skills.AddRange(activeSkillTable.GetActiveSkillsByJob((JobType)i));
        }

        return skills;
    }

    // 스킬 count회 뽑기 
    // 어차피 1뽑, 10뽑만 할거니까 array로
    // 가챠 결과를 구조체로 가지고 UI에게 넘겨줌
    public GachaResult<ActiveSkillSO>[] DrawSkills(int count)
    {
        List<ActiveSkillSO>          skillData = GetSkillDatas();
        GachaResult<ActiveSkillSO>[] results   = new GachaResult<ActiveSkillSO>[count];

        for (int i = 0; i < count; i++)
        {
            ActiveSkillSO skill = gachaManager.Draw(skillData, Define.TierRates); // 하나씩 뽑아서 skill에 저장

            if (skill != null)
            {
                results[i].GachaReward = skill; // 저장한 skill Data는 구조체의 GachaReward에

                AccountManager.Instance.AddSkill(skill, out bool isDuplicate); // 중복이 아니라면 스킬 추가
                results[i].IsDuplicate = isDuplicate;

                // 중복이면 재화 보상 일부 지급
                if (results[i].IsDuplicate)
                {
                    results[i].CompensationAmount = (int)(drawCost * Define.GetCompensationAmount(skill.activeSkillTier));
                    AccountManager.Instance.AddOpal(results[i].CompensationAmount);
                }
            }
            else
            {
                Debug.LogWarning($"{i}번째 뽑기에 실패했습니다.");
            }
        }

        return results;
    }
}