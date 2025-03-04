using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SingleTon_Mono<T> : MonoBehaviour where T : SingleTon_Mono<T>
{
    private static T instance;
    private static bool isApplicationQuitting = false; 

    [SerializeField] private bool persistGlobalScene = true;

    public static T Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning($"[SingleTon_Mono]应用程序退出后请求的实例 {typeof(T)}。 返回null。");
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

                if (instance.persistGlobalScene)
                {
                    DontDestroyOnLoad(instance.gameObject);
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

            if (persistGlobalScene)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            OnAwake();
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[SingleTon_Mono]检测到重复实例 {typeof(T)} 。  删除这个对象。");
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
       
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }
}
