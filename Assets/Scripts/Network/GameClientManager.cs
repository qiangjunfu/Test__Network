using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameClientManager : MonoBehaviour
{
    #region Mono单例

    private static GameClientManager instance;
    private static readonly object lockObj = new object();
    private static bool isApplicationQuitting = false; // 防止退出时创建新实例

    public static GameClientManager Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning($"[GameClientManager] Instance requested after application quit. Returning null.");
                return null;
            }

            lock (lockObj)
            {
                if (instance == null)
                {
                    // 尝试从场景中找到实例
                    instance = FindObjectOfType<GameClientManager>();
                    if (instance == null)
                    {
                        // 如果未找到，自动创建
                        GameObject obj = new GameObject(nameof(GameClientManager));
                        instance = obj.AddComponent<GameClientManager>();
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
            Debug.LogWarning($"[GameClientManager] Duplicate instance detected. Destroying this object.");
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

        OnDestroy2();
    }

    /// <summary>
    /// 初始化逻辑
    /// </summary>
    private void Initialize()
    {
        // 在这里添加初始化逻辑
        Debug.Log("GameClientManager Initialized.");
    }
    #endregion

    [SerializeField] private string serverIp = "10.161.6.104";
    [SerializeField] private int serverPort = 12345;
    [SerializeField] private int clientId = 0;
    //成员类型 1.导演端  2.裁判端  3.操作端1  4.操作端2 
    [SerializeField] private int clientType = 0;
    [SerializeField] private string receiveData = "";
    [SerializeField] private NetworkMessage networkMessage = null;
    [SerializeField] private NetworkMessage networkMessage_last = null;

    private GameClient gameClient;
    public event Action<int> OnClientIdReceived;
    public event Action<NetworkMessage> OnNetworkMessageReceived;

    [SerializeField] private TooltipUI tooltipUI;


    private float heartbeatInterval = 10f;
    private float heartbeatTimer = 0f;



    void Start()
    {
        //ExcelFileManager.Instance.Init();
        //NetworkData networkData = ExcelFileManager.Instance.GetNetworkDataList()[0];
        //JsonFileManager.Instance.Init();
        //NetworkData networkData = JsonFileManager.Instance.GetNetworkDataList()[0];
        JsonUtilityFileManager.Instance.Init();
        NetworkData networkData = JsonUtilityFileManager.Instance.GetNetworkDataList()[0];
        if (networkData != null)
        {
            clientType = networkData.clientType;
            serverIp = networkData.serverIp;
            serverPort = networkData.serverPort;
            tooltipUI?.AddLog($"networkData  serverIp: {networkData.serverIp} , {networkData.serverPort} ");
        }
        else
        {
            tooltipUI.AddLog("Error   networkData == null ");
        }

        gameClient = new GameClient(serverIp, serverPort);
        gameClient.ClientIdAction += ClientIdActionCallback;
        gameClient.ReceiveDataAction += ReceiveDataActionCallback;
        gameClient.ReceiveDataAction_NetworkMessage += ReceiveDataActionCallback__2;
        gameClient.ConnectToServer();
    }

    #region 接收服务器数据处理
    private void ReceiveDataActionCallback__2(NetworkMessage networkMessage)
    {
        this.networkMessage = networkMessage;
        OnNetworkMessageReceived?.Invoke(networkMessage);
    }

    private void ReceiveDataActionCallback(string obj)
    {
        receiveData = obj;
    }

    private void ClientIdActionCallback(int clientId)
    {
        this.clientId = clientId;
        Debug.Log("clientId " + clientId);
        OnClientIdReceived?.Invoke(clientId);
    }

    void ReceiveDataHandle()
    {
        if (networkMessage != networkMessage_last)
        {
            networkMessage_last = networkMessage;

            string receiveData = "";
            switch (networkMessage.MessageType)
            {
                case NetworkMessageType.PositionUpdate:
                    // 反序列化为 PositionUpdateMessage
                    PositionUpdateMessage posMsg = PositionUpdateMessage.FromByteArray(networkMessage.Data);
                    Debug.Log("处理位置更新消息");
                    receiveData = posMsg.PrintInfo();
                    break;

                case NetworkMessageType.CharacterAction:
                    // 反序列化为 CharacterActionMessage
                    CharacterActionMessage actionMsg = CharacterActionMessage.FromByteArray(networkMessage.Data);
                    Debug.Log("处理角色动作消息");
                    receiveData = actionMsg.PrintInfo();
                    break;

                case NetworkMessageType.ObjectSpawn:
                    // 反序列化为 ObjectSpawnMessage
                    ObjectSpawnMessage spawnMsg = ObjectSpawnMessage.FromByteArray(networkMessage.Data);
                    Debug.Log("处理对象生成消息");
                    receiveData = spawnMsg.PrintInfo();
                    break;

                default:
                    Debug.Log("收到未知类型的 NetworkMessage");
                    break;
            }

            tooltipUI?.AddLog(receiveData);
        }
    }

    #endregion


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //SendData1();
            SendData2();
        }

        ReceiveDataHandle();



        heartbeatTimer += Time.deltaTime;
        if (heartbeatTimer >= heartbeatInterval)
        {
            SendHeartbeat();  // 发送心跳包
            heartbeatTimer = 0f;  // 重置计时器
        }
    }




    private void OnDestroy2()
    {
        if (gameClient != null)
        {
            gameClient.Disconnect();
        }
    }


    #region SendData

    public void SendData(NetworkMessage networkMessage)
    {
        // 发送消息
        gameClient.SendNetworkMessage(networkMessage);
    }



    // 使用示例
    void SendData2()
    {
        // 创建一个示例消息，比如角色位置更新
        int x = UnityEngine.Random.Range(0, 10);
        int y = UnityEngine.Random.Range(0, 10);
        int z = UnityEngine.Random.Range(0, 10);
        PositionUpdateMessage positionMessage = new PositionUpdateMessage
        {
            ClientId = clientId,
            ClientType = clientType,
            X = x,
            Y = y,
            Z = z
        };
        //print("客户端发送的数据内容: "  + positionMessage.PrintInfo() ); 
        // 转换为 NetworkMessage
        NetworkMessage networkMessage = new NetworkMessage(NetworkMessageType.PositionUpdate, positionMessage.ToByteArray());


        SendData(networkMessage);
    }


    // 发送心跳包的方法
    private void SendHeartbeat()
    {
        string heartbeatMessage = "HEARTBEAT";
        gameClient.SendMessage(heartbeatMessage);  
        Debug.Log("发送心跳包...");
    }

    #endregion


    #region Log

    #endregion
}
