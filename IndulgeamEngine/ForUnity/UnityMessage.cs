using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace IndulgeamEngine.Network.ForUnity
{
    class UnityMessage
    {
        public static string Encode(Vector3 vector3)
        {
            return vector3.x.ToString() + "," + vector3.y.ToString() + "," + vector3.z.ToString();
        }
        public static Vector3 DecodeVector3(string posInfo)
        {
            string[] p = posInfo.Split(',');
            return new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
        }
        public static string Encode(Quaternion quaternion)
        {
            return Encode(quaternion.eulerAngles);
        }
        public static Quaternion DecodeQuaternion(string quaternionInfo)
        {
            Vector3 rotation= DecodeVector3(quaternionInfo);
            return Quaternion.Euler(rotation);
        }
    }
}
