using System.Collections.Generic;
using UnityEngine;

public class CharacterGachaSystem : MonoBehaviour
{
    [SerializeField] PlayerUnitTable playerUnitTable;

    private int drawCost = 0;
    public int DrawCost => drawCost;

    private GachaManager<PlayerUnitSO> gachaManager;

    private void Awake()
    {
        gachaManager = new GachaManager<PlayerUnitSO>(new RandomCharacterGachaStrategy());
        drawCost = Define.GachaDrawCosts[GachaType.Character]; // UI 및 핸들러에서 항상 올바른 비용을 표시할 수 있도록 미리 초기화
    }

    private List<PlayerUnitSO> GetCharacterDatas()
    {
        List<PlayerUnitSO> characters = new();

        for (int i =0; i < (int)JobType.Monster; i++)
        {
            characters.AddRange(playerUnitTable.GetPlayerUnitsByJob((JobType)i));
        }

        return characters;
    }

    public PlayerUnitSO[] DrawCharacters(int count)
    {
        List<PlayerUnitSO> characterData = GetCharacterDatas();
        PlayerUnitSO[] results = new PlayerUnitSO[count];

        for (int i=0; i<count; i++)
        {
            PlayerUnitSO character = gachaManager.Draw(characterData, Define.TierRates);

            if(character != null)
            {
                results[i] = character;

                AccountManager.Instance.AddPlayerUnit(character);
            }
            else
            {
                Debug.LogWarning($"{i}번째 뽑기에 실패했습니다.");
            }
        }

        return results;
    }
}
