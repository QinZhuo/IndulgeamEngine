using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IndulgeamEngine
{
    class Debug
    {
        public static Action<string> printMethod;
        public static string logShow="";
        static string localTime
        {
            get
            {
                return DateTime.Now.ToLocalTime() + " " + DateTime.Now.Millisecond;
            }
        }
        public static void Log(string info)
        {
            printMethod(localTime + " "+ info);
            logShow = "\n" + localTime + " " + info+ logShow;
        }
        public static void SafeLog(string info)
        {
            printMethod(localTime + "【重要】 " + info);
            logShow = "\n" + localTime + "【重要】 " + info + logShow ;
        }
        public static void Error(string info)
        {
            printMethod(localTime + "【错误】 "+info);
            logShow = "\n"+localTime + "【错误】 " + info + logShow ;
        }
    }
}
