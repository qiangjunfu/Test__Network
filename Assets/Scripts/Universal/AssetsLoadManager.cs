using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.Video;



public class AssetsLoadManager : MonoBehaviour
{
    #region Mono����

    private static AssetsLoadManager instance;
    private static readonly object lockObj = new object();
    private static bool isApplicationQuitting = false; // ��ֹ�˳�ʱ������ʵ��

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
                    // ���Դӳ������ҵ�ʵ��
                    instance = FindObjectOfType<AssetsLoadManager>();
                    if (instance == null)
                    {
                        // ���δ�ҵ����Զ�����
                        GameObject obj = new GameObject(nameof(AssetsLoadManager));
                        instance = obj.AddComponent<AssetsLoadManager>();
                        DontDestroyOnLoad(obj); // ���ֿ糡������
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
            DontDestroyOnLoad(this.gameObject); // ���ֿ糡������
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
    /// ��ʼ���߼�
    /// </summary>
    private void Initialize()
    {
        // ��������ӳ�ʼ���߼�
        Debug.Log("AssetsLoadManager Initialized.");
    }
    #endregion

    private readonly Dictionary<string, Object> cache = new Dictionary<string, Object>();

    // ͨ�ü��ط���
    private T LoadResource<T>(string path) where T : Object
    {
        if (cache.TryGetValue(path, out Object cachedResource) && cachedResource is T)
        {
            return cachedResource as T;
        }

        T resource = Resources.Load<T>(path);
        if (resource == null)
        {
            Debug.LogError($"��Դδ�ҵ�: {path}");
            return null;
        }

        cache[path] = resource;
        return resource;
    }

    // ���� GameObject������ѡ�����ø�����
    public GameObject LoadGameObject(string path, Transform parent = null)
    {
        GameObject prefab = LoadResource<GameObject>(path);
        if (prefab == null) return null;

        GameObject instance = Instantiate(prefab, parent);
        instance.name = path;
        return instance;
    }

    // �������
    public T LoadComponent<T>(string path, Transform parent = null) where T : Component
    {
        GameObject instance = LoadGameObject(path, parent);
        if (instance == null) return null;

        T component = instance.GetComponent<T>();
        if (component == null)
        {
            Debug.LogWarning($"��·�� {path} ��Ԥ����δ�ҵ����: {typeof(T).Name}");
        }
        return component;
    }

    // ���� UI Ԫ��
    public T LoadUIComponent<T>(string path, Transform parent) where T : Component
    {
        T component = LoadComponent<T>(path, parent);
        if (component != null)
        {
            SetRectTransformDefaults(component.GetComponent<RectTransform>());
        }
        return component;
    }

    // ���� RectTransform Ĭ��ֵ
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

    // ������
    public void ClearCache(bool unloadUnusedAssets = false)
    {
        cache.Clear();
        if (unloadUnusedAssets)
        {
            Resources.UnloadUnusedAssets();
        }
        Debug.Log("Resource cache cleared.");
    }


    // �ض���Դ����
    public Texture2D LoadTexture(string path) => LoadResource<Texture2D>(path);
    public AudioClip LoadAudioClip(string path) => LoadResource<AudioClip>(path);
    public VideoClip LoadVideoClip(string path) => LoadResource<VideoClip>(path);
    public Sprite LoadSprite(string path) => LoadResource<Sprite>(path);

    // �������� Sprite
    public Sprite CreateSprite(string path, Rect rect, Vector2 pivot, float pixelsPerUnit = 100.0f)
    {
        Texture2D texture = LoadTexture(path);
        if (texture == null) return null;

        Sprite sprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);
        cache[path] = sprite;
        return sprite;
    }
}
