using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Juwei.JwSocket
{
    /// <summary>
    /// WinForm桌面程序日志处理类
    /// 默认的日志路径为 Application.StartupPath + "/Log/log[*].log"
    /// 默认的日志文件的大小为4MB
    /// </summary>
    public static class WinLog
    {
        private static LogFile s_logFile;

        static WinLog()
        {
            string logDir = Environment.CurrentDirectory + "/Log";
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            s_logFile = new LogFile(logDir + "/log.log");
        }       
      
        private static void Write(string text)
        {
            s_logFile.WriteLine(text);
        }

        /// <summary>
        /// 设置日志文件的路径
        /// </summary>
        /// <param name="filePath">日志文件路径</param>
        /// <exception cref="FileNotFoundException"></exception>
        public static void SetLogFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath + " 文件不存在");

            s_logFile = new LogFile(filePath);
        }
        
        /// <summary>
        /// 设置单个日志文件的大小
        /// </summary>
        /// <param name="maxFileSize">日志文件的最大值，单位字节</param>
        /// <example>4*1024*1024</example>
        public static void SetLogFileSize(int maxFileSize)
        {
            if (maxFileSize == 0)
                throw new ArgumentException("maxFileSize 的长度不能为0");

            s_logFile.MaxFileSize = maxFileSize;
        }

        public static void Error(string message, Exception e)
        {
            string text = 
                "ERROR" + Environment.NewLine + 
                DateTime.Now.ToString() + Environment.NewLine +
                message + Environment.NewLine;
            text += (e == null) ? string.Empty : (e.ToString() + Environment.NewLine);
            Write(text);
        }

        public static void Warning(string message, Exception e)
        {
            string text = 
                "WARNING" + Environment.NewLine + 
                DateTime.Now.ToString() + Environment.NewLine + 
                message + Environment.NewLine;
            text += (e == null) ? string.Empty : (e.ToString() + Environment.NewLine);
            Write(text);
        }

        public static void Info(string message, Exception e)
        {
            string text = 
                "INFO" + Environment.NewLine +
                DateTime.Now.ToString() + Environment.NewLine + 
                message + Environment.NewLine;
            text += (e == null) ? string.Empty : (e.ToString() + Environment.NewLine);
            Write(text);
        }
    }
}
