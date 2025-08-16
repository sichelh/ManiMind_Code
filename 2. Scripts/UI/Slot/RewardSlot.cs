using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemQuantity;


    public void SetRewardItem(RewardData rewardData)
    {
        //TODO : Icon 설정
        itemQuantity.text = $"x{rewardData.Amount}";
    }
}