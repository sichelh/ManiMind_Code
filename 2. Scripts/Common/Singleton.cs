using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object lockObject = new object();
    private static bool isShuttingDown;
    protected bool isDuplicated;

    public static T Instance
    {
        get
        {
            if (isShuttingDown) return null;

            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = (T)FindObjectOfType(typeof(T));

                    if (instance == null)
                    {
                        GameObject singletonObj = new GameObject($"[{typeof(T).Name}]");
                        instance = singletonObj.AddComponent<T>();
                        DontDestroyOnLoad(singletonObj);
                    }
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"Another instance of {typeof(T).Name} already exists! Destroying duplicate.");
            Destroy(gameObject);
            isDuplicated = true;
            return;
        }

        instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        isShuttingDown = true;
        instance = null;
    }
}