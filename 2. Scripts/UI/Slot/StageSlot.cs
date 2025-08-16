using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageSlot : MonoBehaviour
{
    [SerializeField] private GameObject lockImg;
    [SerializeField] private TextMeshProUGUI stageNumberTxt;
    private StageSO stageSo;
    private bool isLocked;

    private UIStageSelect stageSelectUI;

    public void Initialize(StageSO data)
    {
        stageSo = data;
        stageSelectUI = UIManager.Instance.GetUIComponent<UIStageSelect>();
        int chapterId = data.ID / 1000000;     // 예: 1
        int stageNum  = data.ID % 10000 % 100; // 예: 01 ~ 10

        stageNumberTxt.text = $"{chapterId}-{stageNum}";
        int nextStageID = AccountManager.Instance.GetNextStageId(AccountManager.Instance.BestStage);
        isLocked = nextStageID < stageSo.ID;
        lockImg.SetActive(isLocked);
    }

    public void OnClickStageSlot()
    {
        
        if (isLocked)
        {
            
            return;
        }
        
        AudioManager.Instance.PlaySFX(SFXName.SelectedUISound.ToString());

        if (stageSo.HasBeforeDialogue && stageSo.ID > AccountManager.Instance.BestStage)
        {
            DialogueController.Instance.Play(stageSo.beforeDialogueKey, () =>
            {
                stageSelectUI.SetStageInfo(stageSo);
            });
        }

        else
        {
            stageSelectUI.SetStageInfo(stageSo);
        }
    }

    public void OnClickStageLockBtn()
    {
        AudioManager.Instance.PlaySFX(SFXName.NoAccessUISound.ToString());
        PopupManager.Instance.GetUIComponent<ToastMessageUI>().SetToastMessage("입장 조건이 맞지 않아 입장이 불가능합니다.");
    }
}