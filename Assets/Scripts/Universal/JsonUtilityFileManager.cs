using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.VersionControl;
using UnityEngine;

public class JsonUtilityFileManager : MonoBehaviour
{
    #region Mono单例
    private static JsonUtilityFileManager instance;
    private static bool isApplicationQuitting = false;

    [SerializeField] private bool persistGlobalScene = true;

    public static JsonUtilityFileManager Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning($"[SingleTon_Mono]应用程序退出后请求的实例 {typeof(JsonUtilityFileManager)}。 返回null。");
                return null;
            }

            if (instance == null)
            {
                instance = FindObjectOfType<JsonUtilityFileManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject(typeof(JsonUtilityFileManager).Name);
                    instance = go.AddComponent<JsonUtilityFileManager>();
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
            instance = this as JsonUtilityFileManager;

            if (persistGlobalScene)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            OnAwake();
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[SingleTon_Mono]检测到重复实例 {typeof(JsonUtilityFileManager)} 。  删除这个对象。");
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
        Init();
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }
    #endregion

    [SerializeField] private string folderPath;
    //[SerializeField] private List<NetworkData> m_NetworkDataList = new List<NetworkData>();
    //[SerializeField] private List<SheXiangTouData> m_SheXiangTouDataList = new List<SheXiangTouData>();




    public void Init()
    {
        InitClientIdJson();

        folderPath = Path.Combine(Application.streamingAssetsPath, "Jsons");
        LoadAllData();
    }

    public void LoadAllData()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
        FileInfo[] files = directoryInfo.GetFiles("*.json");

        foreach (FileInfo file in files)
        {
            string content = File.ReadAllText(file.FullName); // 读取 JSON 文件内容
            string dataType = Path.GetFileNameWithoutExtension(file.Name); // 使用文件名作为数据类型

            //Debug.Log($"--------- dataType: {dataType}  content: {content}");
            try
            {
                // 根据类型动态反序列化数据
                switch (dataType)
                {
                    //case "NetworkData":
                    //    m_NetworkDataList.AddRange(JsonUtilityArray<NetworkData>(content));
                    //    Debug.Log($"Loaded {m_NetworkDataList.Count} NetworkData entries.");
                    //    break;

                    //case "SheXiangTouData":
                    //    m_SheXiangTouDataList.AddRange(JsonUtilityArray<SheXiangTouData>(content));
                    //    Debug.Log($"Loaded {m_SheXiangTouDataList.Count} SheXiangTouData entries.");
                    //    break;
                    //case "ClientIdMessage":
                    //    m_ClientIdMessageList.AddRange(JsonUtilityArray<ClientIdMessage>(content));
                    //    Debug.Log($"Loaded {m_ClientIdMessageList.Count}  ClientIdMessage entries.");
                    //    break;

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

    //public List<NetworkData> GetNetworkDataList()
    //{
    //    return new List<NetworkData>(m_NetworkDataList);
    //}

    //public List<SheXiangTouData> GetSheXiangTouDataList()
    //{
    //    return new List<SheXiangTouData>(m_SheXiangTouDataList);
    //}


    #endregion


    #region Json

    [Serializable]
    private class Wrapper<T>
    {
        public List<T> data;
    }

    /// <summary>
    /// 自定义解析 JSON 数组的方法
    /// </summary>
    private List<T> JsonUtilityArray<T>(string json)
    {
        string wrappedJson = $"{{ \"data\": {json} }}"; // 包装成对象
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper.data;
    }


    public void SaveDataToFile<T>(List<T> dataList, string filePath)
    {
        try
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"目录 {directoryPath} 已创建");
            }

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close(); // 创建空文件
                Debug.Log($"文件 {filePath} 已创建");
            }


            StringBuilder jsonStringBuilder = new StringBuilder();
            jsonStringBuilder.Append("[\n");
            for (int i = 0; i < dataList.Count; i++)
            {
                string jsonString = JsonUtility.ToJson(dataList[i], true);
                //Debug.Log($"序列化 JSON: {jsonString}");

                // 如果不是最后一个元素，添加逗号
                if (i < dataList.Count - 1)
                {
                    jsonStringBuilder.Append(jsonString + ",\n");
                }
                else
                {
                    jsonStringBuilder.Append(jsonString + "\n");
                }
            }
            jsonStringBuilder.Append("]");
            string finalJson = jsonStringBuilder.ToString();

            File.WriteAllText(filePath, finalJson);

            Debug.Log($"数据已保存到 {filePath} \n{finalJson}");

        }
        catch (Exception ex)
        {
            Debug.LogError($"保存数据时发生错误：{ex.Message}");
        }
    }




    public byte[] JsonToByteArray<T>(T message)
    {
        string jsonString = JsonUtility.ToJson(message);
        //print($"JsonToByteArray : {jsonString}");
        return Encoding.UTF8.GetBytes(jsonString);
    }
    public T ByteArrayToJson<T>(byte[] data)
    {
        string jsonString = Encoding.UTF8.GetString(data);
        //print($"ByteArrayToJson : {jsonString}");
        return JsonUtility.FromJson<T>(jsonString);
    }

    #endregion


    #region 测试
    void InitClientIdJson()
    {
        clientIdPath = Path.Combine(Application.streamingAssetsPath, "Jsons", "ClientIdMessage.json");
        bool isExist = VerifyFileExist(clientIdPath);
        if (!isExist)
        {
            ClientIdMessage clientIdMessage = new ClientIdMessage
            {
                ClientId = -1,
                ClientType = 1,
                GlobalObjId = 1
            };
            List<ClientIdMessage> clientIdMessages = new List<ClientIdMessage>();
            clientIdMessages.Add(clientIdMessage);
            SaveDataToFile<ClientIdMessage>(clientIdMessages, clientIdPath);
        }

    }


    bool VerifyFileExist(string filePath)
    {
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Debug.Log($"目录 {directoryPath} 已创建");
        }

        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close(); // 创建空文件
            Debug.Log($"文件 {filePath} 已创建");

            return false;
        }

        return true;
    }


    string clientIdPath;
    [SerializeField] private List<ClientIdMessage> m_ClientIdMessageList = new List<ClientIdMessage>();
    public List<ClientIdMessage> LoadClientIdMessageJoson()
    {
        string content = File.ReadAllText(clientIdPath);
        Debug.Log($"加载本地Json文件: {clientIdPath} \n " + content);
        List<ClientIdMessage> list = JsonUtilityArray<ClientIdMessage>(content);
        m_ClientIdMessageList = list;
        return list;
    }
    #endregion
}
