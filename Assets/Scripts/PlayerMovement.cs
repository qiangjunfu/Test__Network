using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerMovement  : MonoBehaviour
{
    private bool isMoving = false;  // 角色是否处于移动状态
    private float horizontal = 0f;  // 水平输入
    private float vertical = 0f;    // 垂直输入


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 获取玩家的输入
        float newHorizontal = Input.GetAxis("Horizontal");  // A/D 键，左右移动
        float newVertical = Input.GetAxis("Vertical");      // W/S 键，前后移动

        // 检查输入是否发生变化
        if (newHorizontal != horizontal || newVertical != vertical)
        {
            horizontal = newHorizontal;
            vertical = newVertical;

            // 根据输入变化来决定是否发送新的指令
            if (horizontal != 0 || vertical != 0)
            {
                SendMoveCommandToServer(newHorizontal, newVertical);
            }
            else
            {
                SendStopCommandToServer();
            }
        }
    }

    // 发送角色移动指令到服务器
    private void SendMoveCommandToServer(float horizontal, float vertical)
    {
        string command = $"Move,{horizontal},{vertical}";
        byte[] data = Encoding.UTF8.GetBytes(command);

        int x = UnityEngine.Random.Range(0, 10);
        int y = UnityEngine.Random.Range(0, 10);
        int z = UnityEngine.Random.Range(0, 10);
        PositionUpdateMessage positionMessage = new PositionUpdateMessage
        {
            ClientId = GameClientManager.Instance . clientId,
            ClientType = GameClientManager.Instance . clientType,
            X = x,
            Y = y,
            Z = z
        };
       
        NetworkMessage networkMessage = new NetworkMessage(NetworkMessageType.PositionUpdate, JsonUtilityFileManager.Instance.JsonToByteArray<PositionUpdateMessage>(positionMessage));


        GameClientManager.Instance.SendData(networkMessage);
    }

    // 发送停止移动指令到服务器
    private void SendStopCommandToServer()
    {
        string command = "Stop";
        byte[] data = Encoding.UTF8.GetBytes(command);
        //stream.Write(data, 0, data.Length);  // 将停止指令发送给服务器
    }
}
