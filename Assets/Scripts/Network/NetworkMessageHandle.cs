using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NetworkMessageHandle : MonoBehaviour
{
    #region Mono单例

    private static NetworkMessageHandle instance;
    private static readonly object lockObj = new object();
    private static bool isApplicationQuitting = false;


    public bool persistGlobalScene = true;

    public static NetworkMessageHandle Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning($"[NetworkMessageHandle]应用程序退出后请求的实例。 返回null。");
                return null;
            }

            lock (lockObj)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<NetworkMessageHandle>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject(nameof(NetworkMessageHandle));
                        instance = obj.AddComponent<NetworkMessageHandle>();

                        if (instance.persistGlobalScene)
                        {
                            DontDestroyOnLoad(obj);
                        }
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

            if (persistGlobalScene)
            {
                DontDestroyOnLoad(this.gameObject);
            }
            Initialize();
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[NetworkMessageHandle]检测到重复实例。 删除这个对象。");
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

    private void Initialize()
    {
        Debug.Log("NetworkMessageHandle Initialized.");
    }

    #endregion


    public ConcurrentQueue<NetworkMessage> queue = new ConcurrentQueue<NetworkMessage>();


    private void Start()
    {
        GameClientManager.Instance.OnNetworkMessageReceived += OnNetworkMessageReceived;
    }

    private void OnNetworkMessageReceived(NetworkMessage message)
    {
        queue.Enqueue(message);
    }

    private void MessageProcessing(NetworkMessage message)
    {
        switch (message.MessageType)
        {
            case NetworkMessageType.GetClientId:
                ClientIdMessage clientIdMessage = JsonUtilityFileManager.Instance.ByteArrayToJson<ClientIdMessage>(message.Data);
                //Debug.Log("ClientId : " + clientIdMessage.ClientId);

                // 保存 ClientIdMessage 到本地
                string clientIdPath = Path.Combine(Application.streamingAssetsPath, "Jsons", "ClientIdMessage.json");
                List<ClientIdMessage> clientIdMessageList = new List<ClientIdMessage>();
                clientIdMessageList.Add(clientIdMessage);
                JsonUtilityFileManager.Instance.SaveDataToFile<ClientIdMessage>(clientIdMessageList, clientIdPath);
                break;
            case NetworkMessageType.ObjectSpawn:
                //处理物体生成
                ObjectSpawnMessage objectSpawnMessage = JsonUtilityFileManager.Instance.ByteArrayToJson<ObjectSpawnMessage>(message.Data);
       
                break;
            case NetworkMessageType.TransformUpdate://物体位置与方向发生改变消息
                TransformUpdateMessage transformUpdateMessage = JsonUtilityFileManager.Instance.ByteArrayToJson<TransformUpdateMessage>(message.Data);
                transformUpdateMessage.PrintInfo();
                break;
            case NetworkMessageType.Status://物体状态发生改变的消息
                StatusMessage statusMessage = JsonUtilityFileManager.Instance.ByteArrayToJson<StatusMessage>(message.Data);
                break;
            case NetworkMessageType.JoinRoom://有新的客户端接入消息
                RoomMessage roomMessage = JsonUtilityFileManager.Instance.ByteArrayToJson<RoomMessage>(message.Data);
                break;
            default:
                break;
        }

    }


    private void Update()
    {
        NetworkMessage msg;
        while (queue.TryDequeue(out msg))
        {
            MessageProcessing(msg);
        }
    }
}
