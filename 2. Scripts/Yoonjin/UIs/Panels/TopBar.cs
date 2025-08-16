using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI opalText;

    private AccountManager AccountManager => AccountManager.Instance;

    private void Awake()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void Start()
    {
        AccountManager.OnGoldChanged += UpdateGoldText;
        AccountManager.OnOpalChanged += UpdateOpalText;

        UpdateGoldText(AccountManager.Gold);
        UpdateOpalText(AccountManager.Opal);
    }

    private void OnBackButtonClicked()
    {
        UIManager.Instance.CloseLastOpenedUI();
    }

    public void OnSettingButtonClick()
    {
        PopupManager.Instance.GetUIComponent<SettingPopup>()?.Open();
    }

    public void UpdateGoldText(int gold)
    {
        goldText.text = $"{gold:N0}";
    }

    public void UpdateOpalText(int opal)
    {
        opalText.text = $"{opal:N0}";
    }


    private void OnDisable()
    {
        if (AccountManager != null)
        {
            AccountManager.OnGoldChanged -= UpdateGoldText;
            AccountManager.OnOpalChanged -= UpdateOpalText;
        }
    }
}