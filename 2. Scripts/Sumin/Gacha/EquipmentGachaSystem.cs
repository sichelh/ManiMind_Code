using System.Collections.Generic;
using UnityEngine;

// 전체적인 구조가 SkillGachaSystem이랑 유사.
// 다른점은 중복 재화 보상이 없음. (중복 추가 가능)
// 여기선 사실상 뽑기 결과를 저장하는 구조체도 불필요.

public class EquipmentGachaSystem : MonoBehaviour
{
    [SerializeField] private ItemTable itemTable;

    private int drawCost = 0;
    public int DrawCost => drawCost;

    private GachaManager<EquipmentItemSO> gachaManager;

    private void Awake()
    {
        gachaManager = new GachaManager<EquipmentItemSO>(new RandoomEquipmentGachaStrategy());
        drawCost = Define.GachaDrawCosts[GachaType.Equipment];
    }

    private List<EquipmentItemSO> GetEquipmentDatas()
    {
        List<EquipmentItemSO> equipments = new();

        for (int i = 0; i < (int)JobType.Monster; i++)
        {
            equipments.AddRange(itemTable.GetEquipmentsByJob((JobType)i));
        }

        return equipments;
    }

    public EquipmentItemSO[] DrawEquipments(int count)
    {
        List<EquipmentItemSO> equipmentData = GetEquipmentDatas();
        EquipmentItemSO[]     results       = new EquipmentItemSO[count];

        for (int i = 0; i < count; i++)
        {
            EquipmentItemSO equipment = gachaManager.Draw(equipmentData, Define.TierRates);

            if (equipment != null)
            {
                results[i] = equipment;
                // InventoryManager.Instance.AddItem(new InventoryItem(equipment, 1)); // 이거 맞나?
                InventoryManager.Instance.AddItem(new EquipmentItem(equipment)); // 이게 맞습니다 ㅎ
            }
            else
            {
                Debug.LogWarning($"{i}번째 뽑기에 실패했습니다.");
            }
        }

        return results;
    }
}