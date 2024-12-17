using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;


public class JsonFileManager : MonoBehaviour
{
    #region Mono单例

    private static JsonFileManager instance;
    private static readonly object lockObj = new object();
    private static bool isApplicationQuitting = false; // 防止退出时创建新实例

    public static JsonFileManager Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning($"[JsonFileManager] Instance requested after application quit. Returning null.");
                return null;
            }

            lock (lockObj)
            {
                if (instance == null)
                {
                    // 尝试从场景中找到实例
                    instance = FindObjectOfType<JsonFileManager>();
                    if (instance == null)
                    {
                        // 如果未找到，自动创建
                        GameObject obj = new GameObject(nameof(JsonFileManager));
                        instance = obj.AddComponent<JsonFileManager>();
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
            Debug.LogWarning($"[JsonFileManager] Duplicate instance detected. Destroying this object.");
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
        Debug.Log("JsonFileManager Initialized.");
    }
    #endregion

    [SerializeField, ReadOnly] string folderPath = ""; // JSON文件存放的文件夹路径
    [SerializeField] List<TestData> m_TestDataList = new List<TestData>();
    [SerializeField] List<NetworkData> m_NetworkDataList = new List<NetworkData>();


    public void Init()
    {
        //folderPath = Path.Combine(Application.dataPath, "StreamingAssets/Jsons");
        folderPath = Path.Combine(Application.streamingAssetsPath, "Jsons");

        LoadAllData();
    }


    void LoadAllData()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
        FileInfo[] files = directoryInfo.GetFiles("*.json");
        foreach (FileInfo file in files)
        {
            string content = File.ReadAllText(file.FullName);
            string dataType = Path.GetFileNameWithoutExtension(file.Name);
            try
            {
                switch (dataType)
                {
                    case "TestData":
                        List<TestData> TestDataList = JsonConvert.DeserializeObject<List<TestData>>(content);
                        this.m_TestDataList.AddRange(TestDataList);
                        break;

                    case "NetworkData":
                        List<NetworkData> NetworkDataList = JsonConvert.DeserializeObject<List<NetworkData>>(content);
                        this.m_NetworkDataList.AddRange(NetworkDataList);
                        break;

                    default:
                        Debug.LogWarning($"Unsupported data type {dataType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing file {file.Name}: {ex.Message}");
            }
        }
    }


    #region GetDataList

    public List<TestData> GetTestDataList()
    {
        return new List<TestData>(this.m_TestDataList);
    }
    public List<NetworkData> GetNetworkDataList()
    {
        return new List<NetworkData>(this.m_NetworkDataList);
    }

    #endregion


    #region MyRegion


    #endregion



}
