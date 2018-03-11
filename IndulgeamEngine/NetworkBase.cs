using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.Linq;

namespace IndulgeamEngine.Network
{
    public class NetworkBase 
    {
        public Connection server;
      
        ObjectPool<Connection> connectionPool;
        Socket socket;
        int maxConnect;
        public MessageBase protocol ;
        public Queue<Connection> decodeQueue;
        DecodeMode decodeMode;
        public Dictionary<string, DecodeDelegat> DecodeMethod=new Dictionary<string, DecodeDelegat>();
        public void StartServer(string serverIP, int serverPort, int maxConnect, MessageBase messageProtocol, DecodeMode mode = DecodeMode.QueueWaite)
        {
            if (socket != null)
            {
                Debug.Error("该网络类已被使用无法以服务器模式启动");return;
            }
            protocol = messageProtocol;
            decodeMode = mode;
            if (decodeMode == DecodeMode.QueueWaite)
            {
                decodeQueue = new Queue<Connection>();
            }
            DecodeMethod["Network"] = DecodeNetworkMessage;
            connectionPool = new ObjectPool<Connection>(maxConnect);
            
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress address = IPAddress.Parse(serverIP);
            socket.Bind(new IPEndPoint(address, serverPort));
            socket.Listen(maxConnect);
            socket.BeginAccept(AcceptCallBack, socket);
            Debug.SafeLog("服务器启动成功 接受消息中...");
        }
        public void Connect(string serverIP,int serverPort, MessageBase messageProtocol,DecodeMode mode=DecodeMode.QueueWaite)
        {
            if (socket != null)
            {
                Debug.Error("该网络类已被使用无法用以客户端连接"); return;
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            protocol = messageProtocol;
            try
            {
                decodeMode = mode;
                if (decodeMode == DecodeMode.QueueWaite)
                {
                    decodeQueue = new Queue<Connection>();
                }
                socket.Connect(serverIP, serverPort);
                server = new Connection();
                server.Init(socket);
                socket.BeginReceive(server.readBuff, 0, Connection.BUFFER_SIZE, SocketFlags.None, ReceiveCallBack, server);
                Debug.SafeLog("链接服务器成功 接受消息中...");
            }
            catch (Exception ex)
            {

                Debug.Log(ex.ToString());
            }
        }
        private void ReceiveCallBack(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;
            lock (connection)
            {
                try
                {
                    int count = connection.socket.EndReceive(ar);
                    if (count == 0)
                    {
                        if (server ==null)
                        {
                            CloseConnection(connection);
                        }
                        else
                        {
                            connection.Close();
                            Debug.Error("与服务器断开连接...");
                        }
                        return;
                    }
                    connection.buffCount += count;
                   
                    ProcessData(connection);

                    connection.socket.BeginReceive(connection.readBuff, connection.buffCount,
                         connection.buffRemain, SocketFlags.None, ReceiveCallBack, connection);

                }
                catch (Exception ex)
                {

                    Debug.Log(ex.ToString());
                }
            }
        }
        private void ProcessData(Connection connection)
        {

            if (connection.buffCount < sizeof(Int32))
            {
                Debug.Log("获取不到信息大小重新接包解析：" + connection.buffCount.ToString());
                return;
            }
            Array.Copy(connection.readBuff, connection.lenBytes, sizeof(Int32));
            connection.msgLength = BitConverter.ToInt32(connection.lenBytes, 0);

            if (connection.buffCount < connection.msgLength + sizeof(Int32))
            {
                Debug.Log("信息大小不匹配重新接包解析：" + connection.msgLength.ToString());
                return;
            }

            // Decode(connection);
            MessageBase message = protocol.Decode(connection.readBuff, sizeof(Int32), connection.msgLength);
            Debug.Log("【接收消息】" + message.GetInfo());
            ProcessMessage(connection, message);

            int count = connection.buffCount - connection.msgLength - sizeof(Int32);
            Array.Copy(connection.readBuff, sizeof(Int32) + connection.msgLength, connection.readBuff, 0, count);
            connection.buffCount = count;
            if (connection.buffCount > 0)
            {
                ProcessData(connection);
            }
        }
        private void AcceptCallBack(IAsyncResult ar)
        {
            try
            {
                Debug.Log("开始接受");
                Socket serverSocket = ar.AsyncState as Socket;
                Socket clientSocket = serverSocket.EndAccept(ar);

                Connection client =connectionPool.Get();
                Debug.Log("llalalal");
                if (client==null)
                {

                    clientSocket.Close();
                   
                    Debug.Log("无可用链接");
                 
                }
                else
                {         
                    
                    client.Init(clientSocket);
                    Debug.SafeLog("客户端连接[" + client.addressInfo + "] ");
                    client.socket.BeginReceive(client.readBuff, client.buffCount,
                        client.buffRemain, SocketFlags.None, ReceiveCallBack, client);
                }
                Debug.Log("接收完毕");
                socket.BeginAccept(AcceptCallBack, socket);
            }
            catch (Exception ex)
            {

                Debug.Log(ex.ToString());
            }
        }
        public void Send(Connection connection, MessageBase message)
        {
            if (connection.socket == null) return;
            byte[] bytes = message.Encode();
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendBuff = length.Concat(bytes).ToArray();
            try
            {
                connection.socket.BeginSend(sendBuff, 0, sendBuff.Length, SocketFlags.None, null, null);
                Debug.Log("[消息发送"+ connection.addressInfo+  "]"  + message.GetInfo());
            }
            catch (Exception ex)
            {

                Debug.Log(ex.ToString());
            }

        }
        public void Broadcast(MessageBase message)
        {
            byte[] bytes = message.Encode();
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendBuff = length.Concat(bytes).ToArray();
            for (int i = 0; i < connectionPool.objects.Count; i++)
            {
                connectionPool.objects[i].socket.BeginSend(sendBuff, 0, sendBuff.Length, SocketFlags.None, null, null);
            }
            Debug.Log("[消息广播"+connectionPool.objects.Count+"] " + message.GetInfo());
        }
        protected virtual void ProcessMessage(Connection connection, MessageBase message)
        {
            if (decodeMode == DecodeMode.QueueWaite)
            {
                connection.messageQueue.Enqueue(message);
                decodeQueue.Enqueue(connection);
            }
            else if(decodeMode == DecodeMode.Immediately)
            {
                DecodeMethod[message.GetCommand()](connection, message);
            }
        }
        public void UpdateDecodeMessage(int decodeSum = 10)
        {
            if (decodeMode != DecodeMode.QueueWaite) return;
            for (int i = 0; i < decodeSum; i++)
            {
                if (decodeQueue.Count > 0)
                {
                    Connection connection = decodeQueue.Dequeue();
                    MessageBase message = connection.messageQueue.Dequeue();
                    try
                    {
                       
                        DecodeMethod[message.GetLayer()](connection, message);
                    }
                    catch(Exception ex)
                    {
                        if (DecodeMethod.ContainsKey(message.GetLayer()))
                        {
                            Debug.Error(ex.ToString());
                        }
                        else
                        {
                            Debug.Error("不含有层级[" + message.GetLayer() + "]对应的Decode方法");
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }
        public void CloseConnection(Connection connection)
        {
            Debug.Log("[断开连接]" + connection.addressInfo);
            connectionPool.Delete(connection);
            connection.Close();
        }
        Queue<Connection> close = new Queue<Connection>();
        public void CheckHeartBeat()
        {
            Debug.Log("[心跳检测]");
            close.Clear();
            foreach (var client in connectionPool.objects)
            {
                if (!client.heartBeat)
                {
                    close.Enqueue(client);
                }
                else
                {
                    client.heartBeat = false;
                }
            }
            while (close.Count > 0)
            {
                CloseConnection(close.Dequeue());
            }
        }
        private void Heartbeat(Connection connection)
        {
       
            connection.heartBeat = true;
        }
        public void DecodeNetworkMessage(Connection connection,MessageBase message)
        {
         
            switch (message.GetCommand())
            {
                case "Heartbeat":
                    {
                        Heartbeat(connection);
                        Debug.Log(connection.addressInfo + "心跳");
                        break;
                    }
                default:
                    Debug.Log("无法解析命令：" + message.GetCommand());
                    break;
            }
        }
    }
    public enum DecodeMode
    {
        Immediately,
        QueueWaite,
    }
    public delegate void DecodeDelegat(Connection connection, MessageBase message);
}
