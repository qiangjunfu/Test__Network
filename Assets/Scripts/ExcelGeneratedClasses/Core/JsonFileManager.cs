using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;


public class JsonFileManager : MonoBehaviour
{
    #region Mono����

    private static JsonFileManager instance;
    private static readonly object lockObj = new object();
    private static bool isApplicationQuitting = false; // ��ֹ�˳�ʱ������ʵ��

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
                    // ���Դӳ������ҵ�ʵ��
                    instance = FindObjectOfType<JsonFileManager>();
                    if (instance == null)
                    {
                        // ���δ�ҵ����Զ�����
                        GameObject obj = new GameObject(nameof(JsonFileManager));
                        instance = obj.AddComponent<JsonFileManager>();
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
    /// ��ʼ���߼�
    /// </summary>
    private void Initialize()
    {
        // ��������ӳ�ʼ���߼�
        Debug.Log("JsonFileManager Initialized.");
    }
    #endregion

    [SerializeField, ReadOnly] string folderPath = ""; // JSON�ļ���ŵ��ļ���·��
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
