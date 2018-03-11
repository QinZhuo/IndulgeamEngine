using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IndulgeamEngine.Network.ForUnity
{
    public class ServerBase : MonoBehaviour
    {

        public string host = "127.0.0.1";
        public int port = 12345;
        public int updateDecodeCount = 5;
        public float DecodeTime = 0.05f;
        public float HeartbeatTime = 1;
        public int maxConnect = 4;
        protected NetworkBase network;
        protected bool networkStart = false;


        public void Awake()
        {
            
            if (Debug.printMethod == null)
                Debug.printMethod = print;
          
            StartServer(maxConnect);
        }
        public void StartServer(int maxConnect = 10)
        {
            network = new NetworkBase();
            network.StartServer(host, port, maxConnect, new StringMessageProtocol());
            InvokeRepeating("UpdateDeoode", 0, DecodeTime);
            InvokeRepeating("CheckHeartbeat", 0, HeartbeatTime);
        }
        public void UpdateDeoode()
        {
            network.UpdateDecodeMessage(updateDecodeCount);
        }
        public void CheckHeartbeat()
        {
            network.CheckHeartBeat();
        }
        public void Send(Connection connection, params string[] info)
        {
            MessageBase message = new StringMessageProtocol(info);
            network.Send(connection, message);
        }
        public void Broadcast(params string[] info)
        {
            MessageBase message = new StringMessageProtocol(info);
            network.Broadcast(message);
        }
    }
}
