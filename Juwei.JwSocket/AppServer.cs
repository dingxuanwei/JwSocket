using System;
using System.Collections.Concurrent;
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
        private Timer timer;

        private ConcurrentDictionary<string, AppSession> Sessions = new ConcurrentDictionary<string, AppSession>();

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
            AppSession newSession = new AppSession(handler);
            if (Sessions.ContainsKey(newSession.SessionID)) { AppSession sn = new AppSession(); Sessions.TryRemove(newSession.SessionID, out sn); }
            Sessions.TryAdd(newSession.SessionID, newSession);
            OnNewSessionConnected(new NewSessionConnected(newSession.SessionID, newSession.clientSocket.RemoteEndPoint.ToString()));              //新连接上线
            handler.BeginReceive(newSession.ReceBytes, 0, newSession.ReceBytes.Length, 0, new AsyncCallback(ReadCallback), newSession);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            try
            {
                AppSession session = (AppSession)ar.AsyncState;
                String bytecontent = String.Empty;
                Socket handler = session.clientSocket;
                if (handler != null)
                {
                    if (handler.Connected)
                    {
                        int bytesRead = handler.EndReceive(ar);
                        if (bytesRead > 0)
                        {
                            bytecontent = GetString(session.ReceBytes, bytesRead);
                            LogHelper.WriteLog(typeof(AppServer), "[" + session.SessionID + "]" + ":" + bytecontent);

                            OnReceiveMessage(new SessionReceiveMessage(session.SessionID, session.ReceBytes, bytesRead));            //接收到新的消息
                            handler.BeginReceive(session.ReceBytes, 0, session.ReceBytes.Length, 0, new AsyncCallback(ReadCallback), session);
                        }
                        else
                        {
                            if (Sessions.ContainsKey(session.SessionID))
                            {
                                session.Dispose();
                                AppSession sn = new AppSession();
                                Sessions.TryRemove(session.SessionID, out sn);
                                sn = null;
                                //GC.Collect();
                            }
                            OnSessionDisConnected(new SessionDisConnected(session.SessionID, "客户端已关闭"));
                        }
                    }
                    else
                    {
                        session.Dispose();
                        AppSession sn = new AppSession();
                        Sessions.TryRemove(session.SessionID, out sn);
                        sn = null;
                        //GC.Collect();
                    }
                }
                else {
                    throw new Exception("接收数据时，Socket连接不能为空！");
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

        public AppSession GetSessionByID(string sessionid)
        {
            if (Sessions.ContainsKey(sessionid))
                return Sessions[sessionid];
            else
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
