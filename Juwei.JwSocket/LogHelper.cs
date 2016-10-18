using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juwei.JwSocket
{
    public class LogHelper
    {
        public static void WriteLog(Type type, string errMsg)
        {
            LogFile.GetInstance(type.Name).WriteLine(errMsg + "\r\n");
        }

        public static void WriteLog(Type type, Exception ex)
        {
            LogFile.GetInstance(type.Name).WriteLine(ex.Message + "\r\n" + ex.StackTrace + "\r\n");
        }
    }
}
