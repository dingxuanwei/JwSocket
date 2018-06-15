using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Juwei.JwSocket
{
    public class AppSession : IDisposable
    {
        public Socket clientSocket { get; set; }
        public string SessionID { get; set; }
        public byte[] ReceBytes { get; set; }

        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public AppSession() { }

        public AppSession(Socket socket)
        {
            this.clientSocket = socket;
            this.SessionID = Guid.NewGuid().ToString();
            //ReceBytes = new byte[1024];
        }

        public void Send(string message, Encoding encode, bool isbyte=false)
        {
            if (isbyte)
            {
                byte[] byteData = HexStringToByteArray(message);
                clientSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), clientSocket);
            }
            else
            {
                byte[] byteData = encode.GetBytes(message);
                clientSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), clientSocket);
            }
        }

        public void Send(byte[] bytes)
        {
            clientSocket.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), clientSocket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Dispose()
        {
            try
            {
                this.clientSocket.Shutdown(SocketShutdown.Both);
                this.clientSocket.Close();
                ReceBytes = null;
            }
            catch
            {
            }
        }


        public byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            }
            return buffer;
        }
    }
}
