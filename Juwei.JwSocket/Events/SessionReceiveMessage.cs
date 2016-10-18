using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juwei.JwSocket
{
    /// <summary>
    /// 接收消息事件
    /// </summary>
    public class SessionReceiveMessage : EventArgs
    {
        public string SessionName { get; set; }
        public string Message { get; set; }
        public SessionReceiveMessage(string sessionname, string message)
        {
            this.SessionName = sessionname;
            this.Message = message;
        }
    }
}
