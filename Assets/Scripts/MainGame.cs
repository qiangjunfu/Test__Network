using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;


public class MainGame : MonoBehaviour
{
    public PlayerCtrl playerCtrl;


    #region MyRegion
    void OnEnable()
    {
        GameClientManager.Instance.OnClientIdReceived += OnClientIdReceivedCallback;
    }
    void OnDisable()
    {
        GameClientManager.Instance.OnClientIdReceived -= OnClientIdReceivedCallback;
    }
    private void OnClientIdReceivedCallback(int obj)
    {
        Debug.Log($"连接服务器成功 , 创建角色 --- ");
        //GameObject go  = Resources.Load<GameObject>("PlayerCtrl");
        //playerCtrl = GameObject.Instantiate(go).GetComponent <PlayerCtrl>();


        ObjectSpawnMessage objectSpawnMessage = new ObjectSpawnMessage
        {
            ClientId = GameClientManager.Instance.clientId,
            ClientType = GameClientManager.Instance.clientType,

            ObjectId = 1,
            ObjectType = "2",
            X = 1,
            Y = 1,
            Z = 1
        };
        NetworkMessage networkMessage = new NetworkMessage(NetworkMessageType.ObjectSpawn, JsonUtilityFileManager.Instance.JsonToByteArray<ObjectSpawnMessage>(objectSpawnMessage));
        GameClientManager.Instance.SendData(networkMessage);
    }


    #endregion

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
