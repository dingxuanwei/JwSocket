using System.Text;
using System.Net.Sockets;

namespace Juwei.JwSocket
{
    class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024 * 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }
}
