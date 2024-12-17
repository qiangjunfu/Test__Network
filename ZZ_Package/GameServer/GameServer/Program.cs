using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;


public class GameServer
{
    private static readonly ConcurrentBag<Socket> Clients = new();
    private static readonly ConcurrentDictionary<Socket, int> ClientIds = new();
    private static TcpListener serverListener; // 替换为 TcpListener
    private static int clientIdCounter = 1;
    private const int Port = 12345;
    private const int BufferSize = 1024;
    private static readonly string LogFilePath = "server_log.txt"; // 日志文件路径

    public static async Task Main(string[] args)
    {
        Console.CancelKeyPress += async (sender, e) =>
        {
            e.Cancel = true; // 防止程序直接退出
            await ShutdownServer();
        };

        await StartServer();
    }

    public static async Task StartServer()
    {
        try
        {
            serverListener = new TcpListener(IPAddress.Any, Port); // 使用 TcpListener 初始化
            serverListener.Start();

            string localIP = GetLocalIPAddress();
            Log($"服务器已启动，本机IP：{localIP}，监听端口 {Port}，等待客户端连接...");

            while (true)
            {
                TcpClient tcpClient = await serverListener.AcceptTcpClientAsync();
                Socket clientSocket = tcpClient.Client; // 从 TcpClient 获取底层 Socket
                Clients.Add(clientSocket);

                int clientId = clientIdCounter++;
                ClientIds[clientSocket] = clientId;

                Log($"客户端连接: {clientSocket.RemoteEndPoint}，客户端ID: {clientId}");

                byte[] idMessage = Encoding.UTF8.GetBytes(clientId.ToString());
                await clientSocket.SendAsync(new ArraySegment<byte>(idMessage), SocketFlags.None);
                Log($"已向 {clientSocket.RemoteEndPoint} 发送客户端ID: {clientId}");

                _ = Task.Run(() => HandleClient(clientSocket));
            }
        }
        catch (Exception ex)
        {
            Log($"服务器启动失败: {ex.Message}");
        }
    }

    public static async Task HandleClient(Socket clientSocket)
    {
        byte[] buffer = new byte[BufferSize];
        MemoryStream dataStream = new MemoryStream();

        try
        {
            while (true)
            {
                int bytesReceived = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (bytesReceived == 0)
                {
                    Log($"客户端 {clientSocket.RemoteEndPoint} 主动断开连接");
                    break;
                }

                dataStream.Write(buffer, 0, bytesReceived);

                while (dataStream.Length >= 10)
                {
                    dataStream.Position = 0;

                    byte[] header = new byte[6];
                    dataStream.Read(header, 0, 6);
                    string headerStr = Encoding.UTF8.GetString(header);

                    if (headerStr != "HEADER")
                    {
                        Log($"收到无效包头: {headerStr}");
                        byte[] invalidRemainingData = dataStream.ToArray();
                        dataStream.SetLength(0);
                        if (invalidRemainingData.Length > 6)
                        {
                            dataStream.Write(invalidRemainingData, 6, invalidRemainingData.Length - 6);
                        }
                        break;
                    }

                    byte[] lengthBytes = new byte[4];
                    dataStream.Read(lengthBytes, 0, 4);
                    int bodyLength = BitConverter.ToInt32(lengthBytes, 0);

                    if (dataStream.Length < 10 + bodyLength)
                    {
                        Log("数据包不完整，等待更多数据...");
                        dataStream.Position = dataStream.Length;
                        break;
                    }

                    byte[] body = new byte[bodyLength];
                    dataStream.Read(body, 0, bodyLength);

                    await ProcessMessage(clientSocket, body);

                    byte[] remainingData = dataStream.ToArray();
                    int processedDataLength = 10 + bodyLength;
                    dataStream.SetLength(0);
                    if (remainingData.Length > processedDataLength)
                    {
                        dataStream.Write(remainingData, processedDataLength, remainingData.Length - processedDataLength);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log($"与客户端 {clientSocket.RemoteEndPoint} 通信时发生错误: {ex.Message}");
        }
        finally
        {
            Clients.TryTake(out _);
            ClientIds.TryRemove(clientSocket, out _);
            clientSocket.Dispose();
            Log($"客户端 {clientSocket.RemoteEndPoint} 已断开连接并清理资源。");
        }
    }

    public static async Task ProcessMessage(Socket clientSocket, byte[] message)
    {
        if (TryParseNetworkMessage(message, out NetworkMessage networkMessage))
        {
            Log($"收到来自客户端 {ClientIds[clientSocket]}: {clientSocket.RemoteEndPoint} NetworkMessage: 类型={networkMessage.MessageType}");
            await BroadcastNetworkMessage(clientSocket, networkMessage);
        }
        else
        {
            string receivedMessage = Encoding.UTF8.GetString(message);
            Log($"收到来自客户端 {ClientIds[clientSocket]}: {clientSocket.RemoteEndPoint} 的消息: {receivedMessage}");
            await BroadcastMessage(clientSocket, receivedMessage);
        }
    }

    public static async Task BroadcastMessage(Socket senderSocket, string message)
    {
        byte[] combinedMessage = PrepareMessage(message);

        foreach (var clientSocket in Clients)
        {
            try
            {
                await clientSocket.SendAsync(new ArraySegment<byte>(combinedMessage), SocketFlags.None);
                Log($"广播消息给客户端:  {ClientIds[clientSocket]}: {clientSocket.RemoteEndPoint} ");
            }
            catch (Exception ex)
            {
                Log($"广播消息失败: {ex.Message}");
            }
        }
    }

    public static async Task BroadcastNetworkMessage(Socket senderSocket, NetworkMessage networkMessage)
    {
        byte[] combinedMessage = PrepareNetworkMessage(networkMessage);

        foreach (var clientSocket in Clients)
        {
            try
            {
                await clientSocket.SendAsync(new ArraySegment<byte>(combinedMessage), SocketFlags.None);
                Log($"广播 NetworkMessage 给客户端:  {ClientIds[clientSocket]}: {clientSocket.RemoteEndPoint} ");
            }
            catch (Exception ex)
            {
                Log($"广播 NetworkMessage 失败: {ex.Message}");
            }
        }
    }

    public static byte[] PrepareMessage(string message)
    {
        byte[] header = Encoding.UTF8.GetBytes("HEADER");
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

        byte[] combinedMessage = new byte[header.Length + lengthBytes.Length + messageBytes.Length];
        Array.Copy(header, 0, combinedMessage, 0, header.Length);
        Array.Copy(lengthBytes, 0, combinedMessage, header.Length, lengthBytes.Length);
        Array.Copy(messageBytes, 0, combinedMessage, header.Length + lengthBytes.Length, messageBytes.Length);

        return combinedMessage;
    }

    public static byte[] PrepareNetworkMessage(NetworkMessage networkMessage)
    {
        byte[] header = Encoding.UTF8.GetBytes("HEADER");
        byte[] typeBytes = BitConverter.GetBytes((int)networkMessage.MessageType);
        byte[] dataBytes = networkMessage.Data;
        byte[] lengthBytes = BitConverter.GetBytes(typeBytes.Length + dataBytes.Length);

        byte[] combinedMessage = new byte[header.Length + lengthBytes.Length + typeBytes.Length + dataBytes.Length];
        Array.Copy(header, 0, combinedMessage, 0, header.Length);
        Array.Copy(lengthBytes, 0, combinedMessage, header.Length, lengthBytes.Length);
        Array.Copy(typeBytes, 0, combinedMessage, header.Length + lengthBytes.Length, typeBytes.Length);
        Array.Copy(dataBytes, 0, combinedMessage, header.Length + lengthBytes.Length + typeBytes.Length, dataBytes.Length);

        return combinedMessage;
    }

    public static string GetLocalIPAddress()
    {
        foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    public static async Task ShutdownServer()
    {
        foreach (var clientSocket in Clients)
        {
            clientSocket.Close();
        }
        serverListener.Stop();
        Log("服务器已关闭。");
    }

    public static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now}] {message}");
    }

    public static bool TryParseNetworkMessage(byte[] message, out NetworkMessage networkMessage)
    {
        try
        {
            if (message.Length < 4)
            {
                networkMessage = null;
                return false;
            }

            MessageType messageType = (MessageType)BitConverter.ToInt32(message, 0);
            byte[] data = new byte[message.Length - 4];
            Array.Copy(message, 4, data, 0, data.Length);

            networkMessage = new NetworkMessage(messageType, data);
            return true;
        }
        catch
        {
            networkMessage = null;
            return false;
        }
    }
}

public class NetworkMessage
{
    public MessageType MessageType { get; }
    public byte[] Data { get; }

    public NetworkMessage(MessageType messageType, byte[] data)
    {
        MessageType = messageType;
        Data = data;
    }
}

public enum MessageType
{
    PositionUpdate,
    CharacterAction,
    ObjectSpawn
}
