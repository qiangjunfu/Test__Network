using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class NetworkData
{
    [SerializeField, ReadOnly] public int id;
    [SerializeField, ReadOnly] public int clientId;
    [SerializeField, ReadOnly] public int clientType;
    [SerializeField, ReadOnly] public string serverIp;
    [SerializeField, ReadOnly] public int serverPort;


    public NetworkData() { }
}
