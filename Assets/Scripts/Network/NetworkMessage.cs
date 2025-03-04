using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// 消息类型
/// </summary>
public enum NetworkMessageType
{
    /// <summary>
    /// 位置更新
    /// </summary>
    TransformUpdate,
    /// <summary>
    /// 状态
    /// </summary>
    Status,
    /// <summary>
    /// 物体产生
    /// </summary>
    ObjectSpawn,
    /// <summary>
    /// 新客户端加入的消息
    /// </summary>
    ClientConnect,
    /// <summary>
    /// 客户端退出
    /// </summary>
    ClientDisconnect,


    JoinRoom,
    LeaveRoom,
    SwitchRoom,

    /// <summary>
    /// 服务器获取客户端ID, 如果有则保存会话, 没有则创建id后发送给客户端
    /// </summary>
    GetClientId
}

public class NetworkMessage
{
    public NetworkMessageType MessageType;   // 消息类型
    public byte[] Data;                 // 数据内容


    public NetworkMessage(NetworkMessageType type, byte[] data)
    {
        MessageType = type;
        Data = data;
    }

}

/// <summary>
/// 所有消息的基类，包括客户端ID,成员类型，全局ID及物品类型。
/// </summary>
public abstract class ClientMessageBase
{
    /// <summary>
    /// 客户端 ID，唯一标识
    /// </summary>
    public int ClientId;
    /// <summary>
    /// 成员类型 1.导演端  2.裁判端  3.操作端1  4.操作端2 
    /// </summary>
    public int ClientType;
    /// <summary>
    /// 全局对象 ID，用于标识全局对象
    /// </summary>
    public int GlobalObjId;
    /// <summary>
    /// 物品类型
    /// </summary>
    public int type;

    //public byte[] ToByteArray()
    //{
    //    string jsonString = JsonUtility.ToJson(this);
    //    return Encoding.UTF8.GetBytes(jsonString);
    //}
    //public static T FromByteArray<T>(byte[] data) where T : ClientMessageBase
    //{
    //    string jsonString = Encoding.UTF8.GetString(data);
    //    return JsonUtility.FromJson<T>(jsonString);
    //}
    public virtual string PrintInfo()
    {
        string info = $"客户端ID: {ClientId}, ,类型: {type}, 全局ID: {GlobalObjId}";
        Debug.Log(info);
        return info;
    }
}

/// <summary>
/// 位置与方位发生改变的消息
/// </summary>.l
public class TransformUpdateMessage : ClientMessageBase
{
    public Vector3 position;
    public Quaternion rotation;

    public override string PrintInfo()
    {
        string info = base.PrintInfo() + $", 位置: {position}, 方位: {rotation}";
        Debug.Log(info);
        return info;
    }

}

public class StatusMessage : ClientMessageBase
{
    /// <summary>
    /// 物体当前状态 此处如果用基类变量，再进行Json转换时，经测试无法传递子类，故此处改为传递状态类的Json字符串
    /// </summary>
    public string statusJson;
    public override string PrintInfo()
    {
        string info = base.PrintInfo();
        Debug.Log(info);
        return info;
    }
}

/// <summary>
/// 物体创建消息
/// </summary>
public class ObjectSpawnMessage : ClientMessageBase
{
    public Vector3 position;
    public Quaternion rotation;

    public override string PrintInfo()
    {
        string info = base.PrintInfo() + $", 位置: {position}, 方位: {rotation}";
        Debug.Log(info);
        return info;
    }

}

/// <summary>
/// 新客户加入消息
/// </summary>
public class ClientJoinMessage : ClientMessageBase
{

}


/// <summary>
/// 房间消息 (加入.离开.切换)
/// </summary>
public class RoomMessage : ClientMessageBase
{
    /// <summary>
    /// 房间消息类型 (例子: NetworkMessageType.JoinRoom.ToString();)
    /// </summary>
    public string roomMessageType;
    public string roomId;

    public override string PrintInfo()
    {

        string info = base.PrintInfo() + $", 房间消息类型: {roomMessageType}, 房间id: {roomId}";
        Debug.Log(info);
        return info;
    }
}

/// <summary>
/// 获取客户端ID
/// </summary>
[System.Serializable]
public class ClientIdMessage : ClientMessageBase
{

}