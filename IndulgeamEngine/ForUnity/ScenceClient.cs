using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IndulgeamEngine.Network.ForUnity
{
    public class ScenceClient : ClientBase
    {
        public static ScenceClient instance;
        public NetObject[] netPrefab;
        public Dictionary<string, GameObject> objs = new Dictionary<string, GameObject>();

        private new void Awake()
        {
            base.Awake();
            network.DecodeMethod["Scence"] = DecodeScenceMessage;
            instance = this;
        }
        public static GameObject Instantiate(int prefabIndex, Vector3 position, Quaternion rotation)
        {
            GameObject temp = Instantiate(instance.netPrefab[prefabIndex].gameObject, position, rotation);
            string tempId = temp.name + instance.objs.Count.ToString();
            Debug.Log(tempId);
            //temp.GetComponent<NetObject>().netId = tempId;
            instance.objs.Add(tempId, temp);
            instance.Send("Scence", "Instantiate", prefabIndex.ToString(), UnityMessage.Encode(position),UnityMessage.Encode(rotation), tempId);
            return temp;
        }
        public void DecodeScenceMessage(Connection connection, MessageBase message)
        {
            switch (message.GetCommand())
            {

                case "ChangeId":
                    {
                        // Debug.Log("change" + message.GetInfo(0) + "to" + message.GetInfo(1));
                        string t1 = message.GetInfo(0), t2 = message.GetInfo(1);
                        objs.Add(t2, objs[t1]);
                        objs.Remove(t1);
                        objs[t2].GetComponent<NetObject>().netId = int.Parse( message.GetInfo(1));
                    }
                    break;
                case "SendMessage":
                    {
                        objs[message.GetInfo(0)].SendMessage(message.GetInfo(1), message.GetInfo(2));
                    }
                    break;

                case "Instantiate":
                    {
                        if (objs.ContainsKey(message.GetInfo(3))) { break; }
                        GameObject temp = Instantiate(netPrefab[int.Parse(message.GetInfo(0))].gameObject, UnityMessage.DecodeVector3(message.GetInfo(1)), UnityMessage.DecodeQuaternion(message.GetInfo(2)));
                        objs.Add(message.GetInfo(3), temp);
                        temp.GetComponent<NetObject>().netId =int.Parse(message.GetInfo(3));
                        temp.GetComponent<NetObject>().isLocalObject = false;
                    }
                    break;
                default:
                    break;
            }
        }
        //public static void LoadScenceObject()
        //{
        //    instance.Send("Scence", "LoadScenceObject");
        //}
    }
}
