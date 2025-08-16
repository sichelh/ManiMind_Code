using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StagePanelMonsterSlot : MonoBehaviour
{
    [SerializeField] private Image monsterIcon;


    public void SetMonsterSlot(EnemyUnitSO enemyUnitSo)
    {
        this.gameObject.SetActive(true);
        monsterIcon.sprite = enemyUnitSo.UnitIcon;
    }


    public void EmptySlot()
    {
        monsterIcon.sprite = null;
        this.gameObject.SetActive(false);
    }
}