using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IndulgeamEngine.Network.ForUnity
{
    public class ScenceServer : ServerBase
    {
        public static ScenceServer instance;
        public NetObject[] netPrefab;
        public List<GameObject> objs = new List<GameObject>();
        private new  void  Awake()
        {
            base.Awake();
            instance = this;
            network.DecodeMethod["Scence"] = DecodeScenceMessage;
        }
        public void DecodeScenceMessage(Connection connection, MessageBase message)
        {
            switch (message.GetCommand())
            {
                case "Instantiate":
                    {
                        Debug.Log("Instantiate");
                        GameObject temp = Instantiate(netPrefab[int.Parse(message.GetInfo(0))].gameObject, UnityMessage.DecodeVector3(message.GetInfo(1)),UnityMessage.DecodeQuaternion(message.GetInfo(2)));
                        int tempi = objs.Count;
                        objs.Add(temp);
                        temp.GetComponent<NetObject>().netId = tempi;
                        Send(connection, "Scence", "ChangeId", message.GetInfo(3), tempi.ToString());
                        Broadcast( "Scence", "Instantiate", message.GetInfo(0), message.GetInfo(1),message.GetInfo(2), tempi.ToString());


                    }
                    break;
                case "SendMessage":
                    {
                        objs[int.Parse(message.GetInfo(0))].SendMessage(message.GetInfo(1), message.GetInfo(2));
                    }
                    break;
                default:
                    Debug.SafeLog("无法解析命令：" + message.GetCommand());
                    break;
            }
        }


    }
}
