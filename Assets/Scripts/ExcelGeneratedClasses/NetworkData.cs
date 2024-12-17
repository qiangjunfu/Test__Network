using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class NetworkData
{
    [SerializeField] public int id;
    [SerializeField] public int clientId;
    [SerializeField] public string serverIp;
    [SerializeField] public int serverPort;


    public NetworkData() { }
}
