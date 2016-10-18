using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Juwei.JwSocket
{
    /// <summary>
    /// 日志文件类
    /// </summary>
    public sealed class LogFile
    {
        private string _filePath;
#if DEBUG
        private int _maxFileSize = 4 * 1024 * 1024;
#else
        private int _maxFileSize = 4 * 1024 * 1024;
#endif
        private static object s_lock = new object();      

        /// <summary>
        /// 获取或设置单个日志文件的最大容量
        /// <remarks>Debug模式下，默认日志文件的最大为4KB；Release模式下，默认日志文件的最大为4MB</remarks>
        /// </summary>
        public int MaxFileSize
        {
            get { return this._maxFileSize; }
            set { this._maxFileSize = value; }
        }

        /// <summary>
        /// 实例化日志文件类
        /// </summary>
        /// <param name="filePath">日志文件的全路径</param>
        public LogFile(string filePath)
        {
            this._filePath = filePath;
        }

        public void Write(string text)
        {
            lock (s_lock)
            {
                using (StreamWriter write = new StreamWriter(this._filePath, true, Encoding.UTF8))
                {
                    write.Write(text);
                }
                CheckFile();
            }
        }

        public void WriteLine(string text)
        {
            lock (s_lock)
            {
                using (StreamWriter write = new StreamWriter(this._filePath, true, Encoding.UTF8))
                {
                    write.WriteLine(text);
                }
                CheckFile();
            }
        }

        /// <summary>
        /// 写日志（当前时间、当前执行线程、当前调用堆栈）
        /// </summary>
        /// <param name="logText"></param>
        public void WriteLog(string logText)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(DateTime.Now.ToString());
            sb.AppendLine(string.Format("ThreadId:{0}", System.Threading.Thread.CurrentThread.ManagedThreadId));
            sb.AppendLine(logText);
            sb.AppendLine("----调用堆栈---");
            try { sb.AppendLine(new StackTrace(1).ToString()); }
            catch { }
            sb.AppendLine();
            WriteLine(sb.ToString());
        }
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="logText"></param>
        /// <param name="writeStackTrace">是否写入调用堆栈：True写入，False不写入</param>
        public void WriteLog(string logText, bool writeStackTrace)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(DateTime.Now.ToString());
            sb.AppendLine(string.Format("ThreadId:{0}", System.Threading.Thread.CurrentThread.ManagedThreadId));
            sb.AppendLine(logText);
            if (writeStackTrace)
            {
                sb.AppendLine("----调用堆栈---");
                try { sb.AppendLine(new StackTrace(1).ToString()); }
                catch { }
            }
            sb.AppendLine();
            WriteLine(sb.ToString());
        }

        /// <summary>
        /// 写异常（默认写入当前时间）
        /// </summary>
        /// <param name="ex"></param>
        public void WriteException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(DateTime.Now.ToString());
            sb.AppendLine(string.Format("ThreadId:{0}", System.Threading.Thread.CurrentThread.ManagedThreadId));
            sb.AppendLine(ex.ToString());
            sb.AppendLine();
            WriteLine(sb.ToString());
        }

        #region private

        private void CheckFile()
        {
            FileInfo fileInfo = new FileInfo(this._filePath);
            if (fileInfo.Length > this._maxFileSize)
            {
                ReSetCurrentFile();
            }
        }

        private void ReSetCurrentFile()
        {
            FileInfo fileInfo = new FileInfo(this._filePath);
            string fileNameNoExtension = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);

            int maxLogFileVersion = GetMaxLogVersionByLogFiles();

            int destLogFileVersion = maxLogFileVersion + 1;
            string newFileName = string.Format("{0}/{1}{2}{3}",
                fileInfo.DirectoryName, fileNameNoExtension, destLogFileVersion.ToString(), fileInfo.Extension);

            File.Copy(this._filePath, newFileName);
            File.Delete(this._filePath);
        }

        private int GetMaxLogVersionByLogFiles()
        {
            int result = 0;

            FileInfo fileInfo = new FileInfo(this._filePath);
            string dirPath = fileInfo.DirectoryName;
            string fileNameNoExtension = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);

            string[] files = Directory.GetFiles(dirPath, "*" + fileInfo.Extension);

            //找出类似错误日志的文件列表
            List<string> matchFiles = new List<string>();           
            foreach (string file in files)
            {
                FileInfo fileTemp = new FileInfo(file);
                if (!fileTemp.Name.StartsWith(fileNameNoExtension)) continue;
                matchFiles.Add(file);
            }

            //得到最大的日志文件版本
            int maxVersion = 0;
            foreach (string file in matchFiles)
            {
                int fileVersion = GetLogVersionByLogFile(file);
                maxVersion = (fileVersion > maxVersion) ? fileVersion : maxVersion;
            }

            result = maxVersion;
            return result;
        }

        private int GetLogVersionByLogFile(string file)
        {
            int result = 0;

            FileInfo currentFileInfo = new FileInfo(this._filePath);
            string currentFileNameNoExtension = currentFileInfo.Name.Substring(0, currentFileInfo.Name.Length - currentFileInfo.Extension.Length);

            FileInfo fileInfo = new FileInfo(file);
            string fileNameNoExtension = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);

            string fileVersionStr = fileNameNoExtension.Substring(currentFileNameNoExtension.Length, 
                fileNameNoExtension.Length - currentFileNameNoExtension.Length);

            int fileVersion;
            if (int.TryParse(fileVersionStr, out fileVersion))
            {
                result = fileVersion;
            }

            return result;
        }

        #endregion

        #region Static

        /// <summary>
        /// 根据日志名称获取一个日志对象实例
        /// </summary>
        /// <param name="name">日志对象名称。例如:"sqllog"，那么会在当前目录下创建一个（/Log/sqllog.log）的日志文件</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static LogFile GetInstance(string name)
        {
            return LogFileCreate.GetLog(name);
        }       

        #endregion
    }
}
