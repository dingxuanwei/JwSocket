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
        public string SessionID { get; set; }
        public byte[] ReceBuffer { get; set; }
        public int BufferSize { get; set; }
        public SessionReceiveMessage(string sessionid, byte[] receMsg,int bufferSize)
        {
            this.SessionID = sessionid;
            this.ReceBuffer = receMsg;
            this.BufferSize = bufferSize;
        }
    }
}
