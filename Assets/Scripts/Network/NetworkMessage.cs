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
    public NetworkMessageType MessageType;      // 消息类型
    public byte[] Data;                         // 数据内容


    public NetworkMessage(NetworkMessageType type, byte[] data)
    {
        MessageType = type;
        Data = data;
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

}

public class PositionUpdateMessage : ClientMessageBase
{
    public float X;            // 角色的 X 坐标
    public float Y;            // 角色的 Y 坐标
    public float Z;            // 角色的 Z 坐标


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

   
    public string PrintInfo()
    {
        string str = $"ClientId: {ClientId}, ClientType: {ClientType}, ObjectId: {ObjectId}, ObjectType: {ObjectType}, X: {X}, Y: {Y}, Z: {Z}";
        Debug.Log(str);
        return str;
    }
}

