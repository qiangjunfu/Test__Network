using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameClient
{
    public Action<int> ClientIdAction;
    public Action<string> ReceiveDataAction;
    public Action<NetworkMessage> ReceiveDataAction_NetworkMessage;

    private TcpClient tcpClient; // Changed from Socket to TcpClient
    private NetworkStream networkStream;
    private string serverIp;
    private int serverPort;
    private bool isConnected;

    // 构造函数，初始化服务器地址和端口
    public GameClient(string ip, int port)
    {
        serverIp = ip;
        serverPort = port;
        isConnected = false;
    }

    // 连接到服务器
    public void ConnectToServer()
    {
        try
        {
            // 创建 TCP 连接
            tcpClient = new TcpClient();
            tcpClient.BeginConnect(serverIp, serverPort, new AsyncCallback(OnConnectCallback), tcpClient);
        }
        catch (Exception ex)
        {
            Debug.Log($"连接到服务器时发生错误: {ex.Message}");
        }
    }

    // 连接回调函数
    private void OnConnectCallback(IAsyncResult ar)
    {
        try
        {
            tcpClient.EndConnect(ar);
            networkStream = tcpClient.GetStream(); // 获取网络流
            isConnected = true;
            Debug.Log("已成功连接到服务器。");

            // 接收服务器分配的 ID
            byte[] buffer = new byte[1024];
            networkStream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReceiveCallback), buffer);
        }
        catch (Exception ex)
        {
            Debug.Log($"连接回调时发生错误: {ex.Message}");
        }
    }

    // 接收回调函数
    private void OnReceiveCallback(IAsyncResult ar)
    {
        try
        {
            byte[] buffer = (byte[])ar.AsyncState;
            int bytesRead = networkStream.EndRead(ar);

            if (bytesRead > 0)
            {
                string clientId = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log($"您的客户端 ID 是: {clientId}");
                ClientIdAction?.Invoke(int.Parse(clientId));

                // 启动接收服务器消息的 Task
                ReceiveMessages().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"接收数据时发生错误: {ex.Message}");
        }
    }

    // 发送消息到服务器
    public void SendMessage(string message)
    {
        try
        {
            // 创建包头（固定字符串或协议）
            byte[] header = Encoding.UTF8.GetBytes("HEADER");

            // 将消息转换为字节数组
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // 获取消息体的长度
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

            // 合并包头、包体长度和消息体
            byte[] combinedMessage = new byte[header.Length + lengthBytes.Length + messageBytes.Length];
            Array.Copy(header, 0, combinedMessage, 0, header.Length);
            Array.Copy(lengthBytes, 0, combinedMessage, header.Length, lengthBytes.Length);
            Array.Copy(messageBytes, 0, combinedMessage, header.Length + lengthBytes.Length, messageBytes.Length);

            // 发送完整的数据包
            networkStream.Write(combinedMessage, 0, combinedMessage.Length);
            Debug.Log($"已发送消息: {message}");
        }
        catch (Exception ex)
        {
            Debug.Log($"发送消息时发生错误: {ex.Message}");
        }
    }

    public void SendNetworkMessage(NetworkMessage networkMessage)
    {
        try
        {
            // 创建包头
            byte[] header = Encoding.UTF8.GetBytes("HEADER");

            // 序列化消息类型
            byte[] typeBytes = BitConverter.GetBytes((int)networkMessage.MessageType);

            // 获取数据内容
            byte[] messageBytes = networkMessage.Data;

            // 组合消息体（消息类型 + 数据）
            byte[] messageBody = new byte[typeBytes.Length + messageBytes.Length];
            Array.Copy(typeBytes, 0, messageBody, 0, typeBytes.Length);
            Array.Copy(messageBytes, 0, messageBody, typeBytes.Length, messageBytes.Length);

            // 获取包体长度
            byte[] lengthBytes = BitConverter.GetBytes(messageBody.Length);

            // 组合完整消息（包头 + 包体长度 + 包体）
            byte[] combinedMessage = new byte[header.Length + lengthBytes.Length + messageBody.Length];
            Array.Copy(header, 0, combinedMessage, 0, header.Length);
            Array.Copy(lengthBytes, 0, combinedMessage, header.Length, lengthBytes.Length);
            Array.Copy(messageBody, 0, combinedMessage, header.Length + lengthBytes.Length, messageBody.Length);

            // 发送完整消息
            networkStream.Write(combinedMessage, 0, combinedMessage.Length);
            Debug.Log($"已发送 NetworkMessage: 类型 {networkMessage.MessageType}, 数据大小 {messageBody.Length}");
        }
        catch (Exception ex)
        {
            Debug.Log($"发送消息时发生错误: {ex.Message}");
            if (!tcpClient.Connected)
            {
                Debug.Log("客户端已断开连接，尝试重新连接...");
                ConnectToServer();
            }
        }
    }

    // 使用 Task 接收来自服务器的消息
    public async Task ReceiveMessages()
    {
        try
        {
            byte[] buffer = new byte[1024];

            while (isConnected)
            {
                // 接收包头
                int bytesRead = networkStream.Read(buffer, 0, 6);
                if (bytesRead == 0)
                {
                    Debug.Log("服务器断开连接。");
                    break;
                }

                string header = Encoding.UTF8.GetString(buffer, 0, 6);

                if (header != "HEADER")
                {
                    Debug.Log("收到无效的包头。");
                    continue;
                }

                // 接收包体长度
                bytesRead = networkStream.Read(buffer, 0, 4);
                int messageLength = BitConverter.ToInt32(buffer, 0);

                // 接收包体
                byte[] messageBuffer = new byte[messageLength];
                int totalReceived = 0;

                while (totalReceived < messageLength)
                {
                    bytesRead = networkStream.Read(messageBuffer, totalReceived, messageLength - totalReceived);
                    totalReceived += bytesRead;
                }

                // 尝试解析为 NetworkMessage
                if (TryParseNetworkMessage(messageBuffer, out NetworkMessage networkMessage))
                {
                    Debug.Log($"收到来自服务器的消息 NetworkMessage: 类型={networkMessage.MessageType}, 数据大小={networkMessage.Data.Length}");
                    ReceiveDataAction_NetworkMessage?.Invoke(networkMessage);
                }
                else
                {
                    // 默认处理为字符串消息
                    string receivedMessage = Encoding.UTF8.GetString(messageBuffer);
                    Debug.Log($"收到来自服务器的字符串消息: {receivedMessage}");
                    ReceiveDataAction?.Invoke(receivedMessage);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"接收消息时发生错误: {ex.Message}");
        }
        finally
        {
            Debug.Log("连接已关闭。");
            tcpClient?.Close();
        }
    }

    public void Disconnect()
    {
        isConnected = false;
        tcpClient?.Close();
        Debug.Log("客户端已断开连接。");
    }

    private bool TryParseNetworkMessage(byte[] messageBuffer, out NetworkMessage networkMessage)
    {
        try
        {
            if (messageBuffer.Length >= 4)
            {
                // 前4字节为消息类型
                MessageType messageType = (MessageType)BitConverter.ToInt32(messageBuffer, 0);

                // 剩余部分为数据内容
                byte[] data = new byte[messageBuffer.Length - 4];
                Array.Copy(messageBuffer, 4, data, 0, data.Length);

                networkMessage = new NetworkMessage(messageType, data);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"解析 NetworkMessage 时发生错误: {ex.Message}");
        }

        networkMessage = null;
        return false;
    }
}
