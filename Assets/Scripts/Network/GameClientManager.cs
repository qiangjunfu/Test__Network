using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class GameClientManager : MonoBehaviour
{
    #region Mono单例
    private static GameClientManager instance;
    private static bool isApplicationQuitting = false;

    [SerializeField] private bool persistGlobalScene = true;

    public static GameClientManager Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning($"[SingleTon_Mono]应用程序退出后请求的实例 {typeof(GameClientManager)}。 返回null。");
                return null;
            }

            if (instance == null)
            {
                instance = FindObjectOfType<GameClientManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject(typeof(GameClientManager).Name);
                    instance = go.AddComponent<GameClientManager>();
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
            instance = this as GameClientManager;

            if (persistGlobalScene)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            OnAwake();
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[SingleTon_Mono]检测到重复实例 {typeof(GameClientManager)} 。  删除这个对象。");
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        OnDestroy2();
    }

    protected virtual void OnAwake()
    {

    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    #endregion

    [SerializeField] private string serverIp = "127.0.0.1";
    [SerializeField] private int serverPort = 12345;
    [SerializeField] private int clientId = -1;
    //成员类型 1.导演端  2.裁判端  3.操作端1  4.操作端2 
    [SerializeField] private int clientType = 0;
    [SerializeField] private string receiveData = "";
    [SerializeField] private NetworkMessage networkMessage = null;
    [SerializeField] private NetworkMessage networkMessage_last = null;

    private GameClient gameClient;
    public event Action<int> OnClientIdReceived;
    public event Action<NetworkMessage> OnNetworkMessageReceived;

    [SerializeField] private TooltipUI tooltipUI;
    public int ClientId { get => clientId; }
    public int ClientType { get => clientType; }

    [SerializeField] private bool isConnect = false;
    public bool IsConnect { get => isConnect; }


    void Start()
    {
        StartClient();
    }

    public void StartClient()
    {
        if (isConnect) return;

        //JsonUtilityFileManager.Instance.Init();
        //NetworkData networkData = JsonUtilityFileManager.Instance.GetNetworkDataList()[0];
        //if (networkData != null)
        //{
        //    if (clientType == 0) clientType = networkData.clientType;
        //    serverIp = networkData.serverIp;
        //    serverPort = networkData.serverPort;
        //    tooltipUI?.AddLog($"networkData  serverIp: {networkData.serverIp} , {networkData.serverPort} ");
        //}
        //else
        //{
        //    tooltipUI.AddLog("Error   networkData == null ");
        //}

        gameClient = new GameClient(serverIp, serverPort);
        gameClient.ConnectedServerAction += ConnectedServerCallback;
        gameClient.ReceiveDataAction += ReceiveDataActionCallback;
        gameClient.ReceiveDataAction_NetworkMessage += ReceiveDataActionCallback__NetworkMessage;
        gameClient.ConnectToServer();

    }

    private void OnDestroy2()
    {
        if (gameClient != null)
        {
            gameClient.ConnectedServerAction -= ConnectedServerCallback;
            gameClient.ReceiveDataAction -= ReceiveDataActionCallback;
            gameClient.ReceiveDataAction_NetworkMessage -= ReceiveDataActionCallback__NetworkMessage;
        }

        DisconnectClient();
    }


    #region 接收服务器数据处理
    private void ReceiveDataActionCallback__NetworkMessage(NetworkMessage networkMessage)
    {
        this.networkMessage = networkMessage;
        OnNetworkMessageReceived?.Invoke(networkMessage);


        switch (networkMessage.MessageType)
        {
            case NetworkMessageType.GetClientId:
                ClientIdMessage clientIdMessage = JsonUtilityFileManager.Instance.ByteArrayToJson<ClientIdMessage>(networkMessage.Data);
                Debug.Log("ClientId : " + clientIdMessage.ClientId);
                this.clientId = clientIdMessage.ClientId;
                isConnect = true;

                OnClientIdReceived?.Invoke(this.clientId);

                break;
            default:
                //Debug.Log("收到未知类型的 NetworkMessage");
                break;
        }
    }

    private void ConnectedServerCallback(bool isConnect)
    {
        Debug.Log("连接到服务器:  " + isConnect);

        if (isConnect)
        {
            ClientIdMessage clientIdMessage = JsonUtilityFileManager.Instance.LoadClientIdMessageJoson()[0];
            //clientIdMessage.ClientId = -1;
            if (clientIdMessage.type == -1 || clientIdMessage.type == 0)
            {
                Debug.Log("此电脑第一次打开程序:  type = " + clientIdMessage.type);
                clientIdMessage.ClientId = -1;
                clientIdMessage.ClientType = -1;
                clientIdMessage.GlobalObjId = -1;
                clientIdMessage.type = 1;
            }
            NetworkMessage networkMessage = new NetworkMessage(NetworkMessageType.GetClientId, JsonUtilityFileManager.Instance.JsonToByteArray<ClientIdMessage>(clientIdMessage));

            SendData(networkMessage);
        }
    }

    private void ReceiveDataActionCallback(string obj)
    {
        receiveData = obj;
    }

    #endregion


    void Update()
    {
        if (isConnect == false) { return; }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendData2();
        }

        SendHeartbeat();

    }


    #region 心跳检测
    private float heartbeatInterval = 4f;
    private float heartbeatTimer = 0f;

    private void SendHeartbeat()
    {
        heartbeatTimer += Time.deltaTime;
        if (heartbeatTimer >= heartbeatInterval)
        {
            string heartbeatMessage = "HEARTBEAT";
            gameClient.SendMessage(heartbeatMessage);
            //Debug.Log("发送心跳包...");

            heartbeatTimer = 0f;
        }


    }
    #endregion


    #region SendData

    public void SendData(NetworkMessage networkMessage)
    {
        // 发送消息
        gameClient.SendNetworkMessage(networkMessage);
    }


    // 使用示例
    void SendData2()
    {
        //创建一个示例消息，比如角色位置更新
        int x = UnityEngine.Random.Range(0, 10);
        int y = UnityEngine.Random.Range(0, 10);
        int z = UnityEngine.Random.Range(0, 10);
        TransformUpdateMessage positionMessage = new TransformUpdateMessage
        {
            ClientId = GameClientManager.Instance.ClientId,
            ClientType = GameClientManager.Instance.ClientType,
            GlobalObjId = GameClientManager.Instance.ClientId * 100 + 1,
            type = 1,

            position = new Vector3(x, y, z),
            rotation = Quaternion.Euler(new Vector3(x, y, z))
        };
        //print("客户端发送的数据内容: " + positionMessage.PrintInfo());
        // 转换为 NetworkMessage
        NetworkMessage networkMessage = new NetworkMessage(NetworkMessageType.TransformUpdate, JsonUtilityFileManager.Instance.JsonToByteArray<TransformUpdateMessage>(positionMessage));


        SendData(networkMessage);
    }


    #endregion



    public void DisconnectClient()
    {
        if (gameClient != null)
        {
            gameClient.Disconnect();

            gameClient = null;
            isConnect = false;
        }
    }

    public void SetClientType(int clienttype)
    {
        clientType = clienttype;

        //// 选择成员类型后，连接服务器
        //StartClient();
    }



}
