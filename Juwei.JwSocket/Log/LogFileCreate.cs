using System;
using System.Collections.Generic;
using System.IO;

namespace Juwei.JwSocket
{
    /// <summary>
    /// 日志文件生成器
    /// </summary>
    public static class LogFileCreate
    {
        private static Dictionary<string, LogFile> s_logFileDic = new Dictionary<string, LogFile>();
        private static object s_lock = new object();

        private static Dictionary<string, LogFile> s_exceptionLogFileDic = new Dictionary<string, LogFile>();
        private static object s_lock2 = new object();

        /// <summary>
        /// 根据日志名称获取一个日志对象实例（线程安全）
        /// </summary>
        /// <param name="name">日志对象名称。例如:"sqllog"，那么会在当前目录下创建一个（/Log/sqllog.log）的日志文件</param>
        /// <returns></returns>
        public static LogFile GetLog(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            if (!s_logFileDic.ContainsKey(name))
            {
                lock (s_lock)
                {
                    if (!s_logFileDic.ContainsKey(name))
                    {
                        s_logFileDic[name] = CreateLogFile("Log", name);
                    }
                }
            }               

            return s_logFileDic[name];
        }

        /// <summary>
        /// 根据日志名称获取一个异常日志对象实例（线程安全）
        /// </summary>
        /// <param name="name">日志对象名称。例如:"sqllog"，那么会在当前目录下创建一个（/Exception/sqllog.log）的日志文件</param>
        /// <returns></returns>
        public static LogFile GetExceptionLog(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            if (!s_exceptionLogFileDic.ContainsKey(name))
            {
                lock (s_lock2)
                {
                    if (!s_exceptionLogFileDic.ContainsKey(name))
                    {
                        s_exceptionLogFileDic[name] = CreateLogFile("Exception", name);
                    }
                }
            }

            return s_exceptionLogFileDic[name];
        }

        private static LogFile CreateLogFile(string dirName, string name)
        {
            string logDir = Environment.CurrentDirectory + "/" + dirName;
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            return new LogFile(logDir + string.Format("/{0}.log", name));
        }
    }
}
