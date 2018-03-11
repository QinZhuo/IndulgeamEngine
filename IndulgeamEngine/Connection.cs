using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;

namespace IndulgeamEngine.Network {
    public delegate void DecodeMessage(MessageBase message);
    public class Connection {
        public const int BUFFER_SIZE = 1024;
        public Socket socket;
        public int buffCount ;
        public byte[] readBuff;
        public Int32 msgLength;
        public byte[] lenBytes;
        public Queue<MessageBase> messageQueue;
        public bool heartBeat { get; set; }
        public Connection()
        {
            readBuff = new byte[BUFFER_SIZE];
            lenBytes = new byte[sizeof(Int32)];
            messageQueue = new Queue<MessageBase>();
            buffCount = 0;
            msgLength = 0;
        }
        public void Init(Socket socket)
        {
            this.socket = socket;
            messageQueue.Clear();
            heartBeat = true;
            buffCount = 0;
            msgLength = 0;

        }
        public int buffRemain
        {
            get
            {
                return BUFFER_SIZE - buffCount;
            }
        }
        public string addressInfo
        {
            get
            {
                if (socket != null)
                {
                    return socket.RemoteEndPoint.ToString();
                }
                else
                {
                    return "无socket连接";
                }
            }
        }
        public void Close()
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
            }
        }
       
     
    }
  
}
