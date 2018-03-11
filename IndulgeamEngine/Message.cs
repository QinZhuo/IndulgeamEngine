using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IndulgeamEngine.Network
{
    public class MessageBase
    {
      
        public virtual MessageBase Decode(Byte[] readBuff, int start, int length)
        {
            return new MessageBase();
        }
        public virtual byte[] Encode()
        {
            return new byte[] { };
        }
        public virtual string GetLayer()
        {
            return "";
        }
        public virtual string GetInfo(int i = -1)
        {
            return "";
        }
        public virtual string GetCommand()
        {
            return "";
        }
    }
    public class StringMessageProtocol : MessageBase
    {
        protected string info;
        protected string[] infos;
        public override MessageBase Decode(byte[] readBuff, int start, int length)
        {
            StringMessageProtocol message = new StringMessageProtocol();
            message.info = System.Text.Encoding.UTF8.GetString(readBuff, start, length);
            message.infos = message.info.Split('#');
            return message;
        }
        public override byte[] Encode()
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(info);
            return bytes;
        }
        public override string GetInfo(int i = -1)
        {
            if (i < 0)
            {
                return info;
            }
            else
            {
                return infos[i + 2];
            }
        }
        public override string GetLayer()
        {
            if (info.Length == 0) return "";
            return infos[0];
        }
        public override string GetCommand()
        {
            return infos[1];
        }

        public StringMessageProtocol(params string[] message)
        {
            info = "";
            infos = message;
            if (message.Length == 0) return;
            for (int i = 0; i < message.Length; i++)
            {

                if (i == message.Length - 1)
                {
                    info += message[i];
                }
                else
                {
                    info += message[i] + "#";
                }
            }
        }
    }
}
