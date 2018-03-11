using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IndulgeamEngine.Network.ForUnity
{
    public class ClientBase : MonoBehaviour
    {

        public string host = "127.0.0.1";
        public int port = 12345;
        public int updateDecodeCount = 5;
        public float DecodeTime = 0.05f;
        public float HeartbeatTime = 1;
        protected NetworkBase network;
        protected bool networkStart = false;
        public void Awake()
        {
            if (Debug.printMethod == null)
                Debug.printMethod = print;
            StartClient();
        }
        public void StartClient()
        {
            network = new NetworkBase();
            network.Connect(host, port, new StringMessageProtocol());
            InvokeRepeating("UpdateDeoode", 0, DecodeTime);
            InvokeRepeating("Heartbeat", 0, HeartbeatTime);
        }
        public void UpdateDeoode()
        {
            network.UpdateDecodeMessage(updateDecodeCount);
        }
        public void Heartbeat()
        {
            Send("Network", "Heartbeat");
        }
        public void Send(params string[] info)
        {
            MessageBase message = new StringMessageProtocol(info);
            network.Send(network.server, message);
        }
    }
}
