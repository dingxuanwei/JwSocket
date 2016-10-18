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
        public string SessionName { get; set; }
        public NewSessionConnected(string sessionname)
        {
            this.SessionName = sessionname;
        }
    }
}
