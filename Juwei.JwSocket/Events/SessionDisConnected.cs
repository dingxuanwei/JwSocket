using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juwei.JwSocket
{
    /// <summary>
    /// 连接关闭事件
    /// </summary>
    public class SessionDisConnected:EventArgs
    {
        public string SessionID { get; set; }
        public string CloseReason { get; set; }

        public SessionDisConnected(string sessionid,string closereason)
        {
            this.SessionID = sessionid;
            this.CloseReason = closereason;
        }
    }
}
