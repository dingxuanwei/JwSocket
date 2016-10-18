using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Juwei.JwSocket;
using System.Net;

namespace SocketDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            AppServer appServer = new AppServer(IPAddress.Any,8888);
            appServer.NewSessionConnected += AppServer_NewSessionConnected;
            appServer.ReceiveMessage += AppServer_ReceiveMessage;
            appServer.SessionDisConnected += AppServer_SessionDisConnected;
            appServer.ServerStart();

            Console.ReadLine();
        }
        private static void AppServer_NewSessionConnected(NewSessionConnected connected)
        {
            Console.WriteLine("New Session:" + connected.SessionName);
        }
        private static void AppServer_ReceiveMessage(SessionReceiveMessage msg)
        {
            Console.WriteLine("Receive New Message:" + Encoding.ASCII.GetString(msg.ReceBuffer, 0, msg.BufferSize));
        }

        private static void AppServer_SessionDisConnected(SessionDisConnected disconnected)
        {
            Console.WriteLine("Session " + disconnected.SessionID + " Closed.Resean:" + disconnected.CloseReason);
        }
    }
}
