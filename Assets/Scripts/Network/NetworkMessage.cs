using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;



public enum NetworkMessageType
{
    PositionUpdate,
    CharacterAction,
    ObjectSpawn
}

public class NetworkMessage
{
    public NetworkMessageType MessageType;   // 消息类型
    public byte[] Data;               // 数据内容


    public NetworkMessage(NetworkMessageType type, byte[] data)
    {
        MessageType = type;
        Data = data;
    }


    public static void HandleMessage(NetworkMessage message)
    {
        switch (message.MessageType)
        {
            case NetworkMessageType.PositionUpdate:
                PositionUpdateMessage posMsg = PositionUpdateMessage.FromByteArray(message.Data);
                // 更新角色位置
                break;

            case NetworkMessageType.CharacterAction:
                CharacterActionMessage actionMsg = CharacterActionMessage.FromByteArray(message.Data);
                // 执行角色操作
                break;

            case NetworkMessageType.ObjectSpawn:
                ObjectSpawnMessage spawnMsg = ObjectSpawnMessage.FromByteArray(message.Data);
                // 生成物体
                break;

            default:
                Debug.Log("Unknown message type");
                break;
        }
    }


}



public class ClientMessageBase
{
    /// <summary>
    /// 客户端 ID，唯一标识
    /// </summary>
    public int ClientId; 
    /// <summary>
    /// 成员类型 1.导演端  2.裁判端  3.操作端1  4.操作端2 
    /// </summary>
    public int ClientType;

    //public byte[] ToByteArray()
    //{
    //    var data = new List<byte>();
    //    data.AddRange(BitConverter.GetBytes(ClientId));
    //    data.AddRange(BitConverter.GetBytes(ClientType));
    //    return data.ToArray();
    //}

    //public  ClientMessageBase FromByteArray(byte[] data) 
    //{
    //    var message = new ClientMessageBase();
    //    message.ClientId = BitConverter.ToInt32(data, 0);
    //    message.ClientType = BitConverter.ToInt32(data, 4);
    //    return message;
    //}
}


public class PositionUpdateMessage : ClientMessageBase
{
    public float X;            // 角色的 X 坐标
    public float Y;            // 角色的 Y 坐标
    public float Z;            // 角色的 Z 坐标


    // 序列化为字节流
    public byte[] ToByteArray()
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(ClientId));
        data.AddRange(BitConverter.GetBytes(ClientType));
        data.AddRange(BitConverter.GetBytes(X));
        data.AddRange(BitConverter.GetBytes(Y));
        data.AddRange(BitConverter.GetBytes(Z));
        return data.ToArray();
    }

    // 从字节流反序列化
    public static PositionUpdateMessage FromByteArray(byte[] data)
    {
        var message = new PositionUpdateMessage();
        message.ClientId = BitConverter.ToInt32(data, 0);
        message.ClientType = BitConverter.ToInt32(data, 4);
        message.X = BitConverter.ToSingle(data, 8);
        message.Y = BitConverter.ToSingle(data, 12);
        message.Z = BitConverter.ToSingle(data, 16);
        return message;
    }


    public string PrintInfo()
    {
        string str = $"ClientId: {ClientId} , ClientType: {ClientType} , X: {X} , Y: {Y} , Z: {Z} ";
        Debug.Log(str);
        return str;
    }
}


public class CharacterActionMessage : ClientMessageBase
{
    public string ActionType;   // 动作类型，例如 "Jump", "Attack" 等
    public float Timestamp;     // 时间戳，可以用于同步操作

    public byte[] ToByteArray()
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(ClientId));
        data.AddRange(BitConverter.GetBytes(ClientType));
        data.AddRange(Encoding.UTF8.GetBytes(ActionType));
        data.AddRange(BitConverter.GetBytes(Timestamp));
        return data.ToArray();
    }

    public static CharacterActionMessage FromByteArray(byte[] data)
    {
        var message = new CharacterActionMessage();
        message.ClientId = BitConverter.ToInt32(data, 0);
        message.ClientType = BitConverter.ToInt32(data, 4);
        message.ActionType = Encoding.UTF8.GetString(data, 8, data.Length - 12);
        message.Timestamp = BitConverter.ToSingle(data, data.Length - 4);
        return message;
    }

    public string PrintInfo()
    {
        string str = $"ClientId: {ClientId}, ClientType: {ClientType}, ActionType: {ActionType}, Timestamp: {Timestamp}";
        Debug.Log(str);
        return str;
    }
}


public class ObjectSpawnMessage  : ClientMessageBase
{
    public int ObjectId;       // 物体的 ID
    public string ObjectType;  // 物体的类型，可能是 "Tree", "Rock" 等
    public float X;            // 物体的初始 X 坐标
    public float Y;            // 物体的初始 Y 坐标
    public float Z;            // 物体的初始 Z 坐标

    public byte[] ToByteArray()
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(ClientId));
        data.AddRange(BitConverter.GetBytes(ClientType));
        data.AddRange(BitConverter.GetBytes(ObjectId));
        data.AddRange(Encoding.UTF8.GetBytes(ObjectType));
        data.AddRange(BitConverter.GetBytes(X));
        data.AddRange(BitConverter.GetBytes(Y));
        data.AddRange(BitConverter.GetBytes(Z));
        return data.ToArray();
    }

    public static ObjectSpawnMessage FromByteArray(byte[] data)
    {
        var message = new ObjectSpawnMessage();
        message.ClientId = BitConverter.ToInt32(data, 0);
        message.ClientType = BitConverter.ToInt32(data, 4);
        message.ObjectId = BitConverter.ToInt32(data, 8);
        message.ObjectType = Encoding.UTF8.GetString(data, 12, data.Length - 24);
        message.X = BitConverter.ToSingle(data, data.Length - 12);
        message.Y = BitConverter.ToSingle(data, data.Length - 8);
        message.Z = BitConverter.ToSingle(data, data.Length - 4);
        return message;
    }

    public string PrintInfo()
    {
        string str = $"ClientId: {ClientId}, ClientType: {ClientType}, ObjectId: {ObjectId}, ObjectType: {ObjectType}, X: {X}, Y: {Y}, Z: {Z}";
        Debug.Log(str);
        return str;
    }
}

