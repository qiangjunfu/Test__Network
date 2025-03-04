using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.Video;



public class AssetsLoadManager : MonoBehaviour
{
    #region Mono单例

    private static AssetsLoadManager instance;
    private static readonly object lockObj = new object();
    private static bool isApplicationQuitting = false; // 防止退出时创建新实例

    public static AssetsLoadManager Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning($"[AssetsLoadManager] Instance requested after application quit. Returning null.");
                return null;
            }

            lock (lockObj)
            {
                if (instance == null)
                {
                    // 尝试从场景中找到实例
                    instance = FindObjectOfType<AssetsLoadManager>();
                    if (instance == null)
                    {
                        // 如果未找到，自动创建
                        GameObject obj = new GameObject(nameof(AssetsLoadManager));
                        instance = obj.AddComponent<AssetsLoadManager>();
                        DontDestroyOnLoad(obj); // 保持跨场景存在
                    }
                }
                return instance;
            }
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject); // 保持跨场景存在
            Initialize();
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[AssetsLoadManager] Duplicate instance detected. Destroying this object.");
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    /// 初始化逻辑
    /// </summary>
    private void Initialize()
    {
        // 在这里添加初始化逻辑
        Debug.Log("AssetsLoadManager Initialized.");
    }
    #endregion

    private readonly Dictionary<string, Object> cache = new Dictionary<string, Object>();

    // 通用加载方法
    private T LoadResource<T>(string path) where T : Object
    {
        if (cache.TryGetValue(path, out Object cachedResource) && cachedResource is T)
        {
            return cachedResource as T;
        }

        T resource = Resources.Load<T>(path);
        if (resource == null)
        {
            Debug.LogError($"资源未找到: {path}");
            return null;
        }

        cache[path] = resource;
        return resource;
    }

    // 加载 GameObject，并可选择设置父对象
    public GameObject LoadGameObject(string path, Transform parent = null)
    {
        GameObject prefab = LoadResource<GameObject>(path);
        if (prefab == null) return null;

        GameObject instance = Instantiate(prefab, parent);
        instance.name = path;
        return instance;
    }

    // 加载组件
    public T LoadComponent<T>(string path, Transform parent = null) where T : Component
    {
        GameObject instance = LoadGameObject(path, parent);
        if (instance == null) return null;

        T component = instance.GetComponent<T>();
        if (component == null)
        {
            Debug.LogWarning($"在路径 {path} 的预设中未找到组件: {typeof(T).Name}");
        }
        return component;
    }

    // 加载 UI 元素
    public T LoadUIComponent<T>(string path, Transform parent) where T : Component
    {
        T component = LoadComponent<T>(path, parent);
        if (component != null)
        {
            SetRectTransformDefaults(component.GetComponent<RectTransform>());
        }
        return component;
    }

    // 设置 RectTransform 默认值
    private void SetRectTransformDefaults(RectTransform rectTransform)
    {
        if (rectTransform == null) return;

        rectTransform.anchoredPosition3D = Vector3.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
    }

    // 清理缓存
    public void ClearCache(bool unloadUnusedAssets = false)
    {
        cache.Clear();
        if (unloadUnusedAssets)
        {
            Resources.UnloadUnusedAssets();
        }
        Debug.Log("Resource cache cleared.");
    }


    // 特定资源加载
    public Texture2D LoadTexture(string path) => LoadResource<Texture2D>(path);
    public AudioClip LoadAudioClip(string path) => LoadResource<AudioClip>(path);
    public VideoClip LoadVideoClip(string path) => LoadResource<VideoClip>(path);
    public Sprite LoadSprite(string path) => LoadResource<Sprite>(path);

    // 从纹理创建 Sprite
    public Sprite CreateSprite(string path, Rect rect, Vector2 pivot, float pixelsPerUnit = 100.0f)
    {
        Texture2D texture = LoadTexture(path);
        if (texture == null) return null;

        Sprite sprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);
        cache[path] = sprite;
        return sprite;
    }
}
