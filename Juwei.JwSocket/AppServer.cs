using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Juwei.JwSocket
{
    public class AppServer : IDisposable
    {
        public IPAddress ServerAddress { get; set; }
        public int ServerPort { get; set; }
        private Socket socketServer = null;
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private Thread thread = null;

        private Dictionary<string, AppSession> Sessions = new Dictionary<string, AppSession>();

        #region 新连接上线事件
        public delegate void NewSessionConnectedHandler(NewSessionConnected connected);
        public event NewSessionConnectedHandler NewSessionConnected;
        /// <summary>
        /// 触发接收消息事件
        /// </summary>
        /// <param name="message">消息内容</param>
        protected void OnNewSessionConnected(NewSessionConnected connected)
        {
            if (NewSessionConnected != null)
            {
                NewSessionConnected(connected);
            }
        }
        #endregion

        #region 接收到消息事件
        public delegate void ReceiveMessageHandler(SessionReceiveMessage msg);
        public event ReceiveMessageHandler ReceiveMessage;
        /// <summary>
        /// 触发接收消息事件
        /// </summary>
        /// <param name="message">消息内容</param>
        protected void OnReceiveMessage(SessionReceiveMessage msg)
        {
            if (ReceiveMessage != null)
            {
                ReceiveMessage(msg);
            }
        }
        #endregion

        #region 连接下线事件
        public delegate void SessionDisConnectedHandler(SessionDisConnected disconnected);
        public event SessionDisConnectedHandler SessionDisConnected;
        /// <summary>
        /// 触发接收消息事件
        /// </summary>
        /// <param name="message">消息内容</param>
        protected void OnSessionDisConnected(SessionDisConnected disconnected)
        {
            if (SessionDisConnected != null)
            {
                SessionDisConnected(disconnected);
            }
        }
        #endregion

        public AppServer(IPAddress ip, int port)
        {
            this.ServerAddress = ip;
            this.ServerPort = port;
        }

        public void ServerStart()
        {
            try
            {
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(ServerAddress, ServerPort);
                socketServer.Bind(endPoint);
                socketServer.Listen(100);

                thread = new Thread(ServerRunListen);
                thread.Start();
            }
            catch (Exception e)
            {

                LogHelper.WriteLog(typeof(AppServer), e);
            }
        }

        private void ServerRunListen()
        {
            try
            {
                while (true)
                {
                    allDone.Reset();
                    Console.WriteLine("Waiting for a connection...");
                    socketServer.BeginAccept(new AsyncCallback(AcceptCallback), socketServer);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog(typeof(AppServer), e);
              //  Console.WriteLine(e.ToString());
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject();
            state.workSocket = handler;
            if (Sessions.ContainsKey(handler.RemoteEndPoint.ToString())) Sessions.Remove(handler.RemoteEndPoint.ToString());
            Sessions.Add(handler.RemoteEndPoint.ToString(), new AppSession(handler));
            OnNewSessionConnected(new NewSessionConnected(handler.RemoteEndPoint.ToString()));              //新连接上线
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            try
            {
                String content = String.Empty;
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;
                if (handler != null)
                {
                    int bytesRead = handler.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        content = GetString(state.buffer, bytesRead);
                        LogHelper.WriteLog(typeof(AppServer), "原始[" + handler.RemoteEndPoint.ToString() + "]" + ":"
                    + content);
                        string str = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                        if (str.Contains("GET"))
                        { }
                        else
                        {
                            //   state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                            // content = state.sb.ToString();
                            OnReceiveMessage(new SessionReceiveMessage(handler.RemoteEndPoint.ToString(), content));            //接收到新的消息
                            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                            state.sb.Clear();
                        }
                    }
                    else
                    {
                        if (Sessions.ContainsKey(handler.RemoteEndPoint.ToString())) Sessions.Remove(handler.RemoteEndPoint.ToString());
                        OnSessionDisConnected(new SessionDisConnected(handler.RemoteEndPoint.ToString(), "客户端已关闭"));
                    }
                }
            }
            //   catch (System.ObjectDisposedException)
            catch (Exception e)
            {
                LogHelper.WriteLog(typeof(AppServer), e);
            }
        }

        public void Dispose()
        {
            try
            {
                if (socketServer != null && socketServer.Connected)
                {
                    this.socketServer.Shutdown(SocketShutdown.Both);
                    this.socketServer.Close();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(AppServer), ex);
                // Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 关闭所有连接
        /// </summary>
        public void CloseAllSessions()
        {
            try
            {
                //foreach (var item in Sessions)
                //{
                //    item.Value.Dispose();
                //}
                //socketServer.Close();
                thread.Abort();
                thread = null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(AppServer), ex);
                //Console.WriteLine(ex.Message);
            }
        }

        public AppSession GetSessionByName(string sessionName)
        {
            if (Sessions.ContainsKey(sessionName))
            {
                return Sessions[sessionName];
            }
            return null;
        }



        private string GetString(byte[] bt, int iLength)
        {
            string str = "";

            for (int i = 0; i < iLength; i++)
            {
                str += Convert.ToInt32(bt[i]).ToString("X2") + " ";
            }

            return str;
        }

    }
}
