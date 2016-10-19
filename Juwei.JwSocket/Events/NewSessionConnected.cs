using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juwei.JwSocket
{
    /// <summary>
    /// 新连接事件
    /// </summary>
    public class NewSessionConnected : EventArgs
    {
        public string SessionID { get; set; }
        public string IP { get; set; }
        public NewSessionConnected(string sessionid,string ip)
        {
            this.SessionID = sessionid;
            this.IP = ip;
        }
    }
}
