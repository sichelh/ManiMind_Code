using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    private readonly Dictionary<Type, UIBase> uiDict = new();
    private List<UIBase> openedUIList = new();

    private Camera mainCamera;

    protected override void Awake()
    {
        base.Awake();
        if (!isDuplicated)
        {
            LoadSceneManager.Instance.OnLoadingCompleted += InitializeUIRoot;
            InitializeUIRoot();
        }
    }

    public void InitializeUIRoot()
    {
        Transform uiRoot = GameObject.Find("UIRoot")?.transform;
        if (uiRoot == null)
        {
            Debug.LogWarning("[UIManager] UIRoot를 찾을 수 없습니다.");
            return;
        }

        openedUIList.Clear();
        uiDict.Clear();
        mainCamera = Camera.main;
        UIBase[]         uiComponents = uiRoot.GetComponentsInChildren<UIBase>(true);
        List<GameObject> toDestroy    = new();
        foreach (UIBase uiComponent in uiComponents)
        {
            Type type = uiComponent.GetType();
            if (!uiDict.TryAdd(type, uiComponent))
            {
                toDestroy.Add(uiComponent.gameObject);
            }
            else
            {
                uiComponent.Close();
            }
        }


        foreach (GameObject go in toDestroy)
        {
            Destroy(go);
        }
    }

    public void Open(UIBase ui, bool isFull = true)
    {
        if (!openedUIList.Contains(ui))
        {
            openedUIList.Add(ui);
            AudioManager.Instance.PlaySFX(SFXName.OpenUISound.ToString());
        }

        ui?.Open();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null && mainCamera.enabled && isFull)
        {
            mainCamera.enabled = false;
        }
    }

    public void Close(UIBase ui)
    {
        if (openedUIList.Contains(ui))
        {
            openedUIList.Remove(ui);
            AudioManager.Instance.PlaySFX(SFXName.CloseUISound.ToString());
            ui.Close();
        }
    }

    public void CloseAll()
    {
        foreach (UIBase ui in openedUIList)
        {
            ui.Close();
        }
    }

    public void CloseLastOpenedUI()
    {
        if (openedUIList.Count == 0)
        {
            TwoChoicePopup popup = PopupManager.Instance.GetUIComponent<TwoChoicePopup>();
            popup.SetAndOpenPopupUI("종료하기",
                "게임을 종료하시겠습니까?",
                () =>
                {
                    SaveLoadManager.Instance.HandleApplicationQuit();
                },
                null,
                "종료하기");
            return;
        }
        else if (openedUIList.Count == 1)
        {
            mainCamera.enabled = true;
        }

        UIBase ui = openedUIList.Last();
        Close(ui);
    }

    public T GetUIComponent<T>() where T : UIBase
    {
        return uiDict.TryGetValue(typeof(T), out UIBase ui) ? ui as T : null;
    }
}

public class UIBase : MonoBehaviour
{
    [SerializeField] private RectTransform contents;

    protected RectTransform Contents  => contents;
    protected UIManager     UIManager => UIManager.Instance;

    public virtual void Open()
    {
        contents.gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        contents.gameObject.SetActive(false);
    }

    public void OnClickCloseBtn()
    {
        UIManager.Instance.Close(this);
    }
}