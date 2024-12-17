using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SingleTon_Mono<T> : MonoBehaviour where T : SingleTon_Mono<T>
{
    private static T instance;
    private static bool isApplicationQuitting = false; // 防止退出时创建新实例

    // 是否保留跨场景的单例实例
    [SerializeField]
    private bool dontDestroyOnLoad = true;

    public static T Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning($"[SingleTon_Mono] Instance of {typeof(T)} is requested after application quitting.");
                return null;
            }

            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    instance = go.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            OnAwake();
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[SingleTon_Mono] Duplicate instance of {typeof(T)} found. Destroying this instance.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    protected virtual void OnAwake()
    {
        // 子类重写以添加额外的初始化逻辑
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }
}
