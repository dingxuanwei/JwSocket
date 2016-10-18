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
        public string SessionName { get; set; }
        public string CloseReason { get; set; }

        public SessionDisConnected(string sessionname,string closereason)
        {
            this.SessionName = sessionname;
            this.CloseReason = closereason;
        }
    }
}
