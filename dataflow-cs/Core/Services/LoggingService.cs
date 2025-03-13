using dataflow_cs.Core.Interfaces;
using System;
using System.Diagnostics;
using System.IO;

namespace dataflow_cs.Core.Services
{
    /// <summary>
    /// 日志服务实现
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly string _logFilePath;
        private static LoggingService _instance;

        /// <summary>
        /// 获取日志服务单例
        /// </summary>
        public static LoggingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoggingService();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 私有构造函数，确保单例模式
        /// </summary>
        private LoggingService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string logDirectory = Path.Combine(appDataPath, "DataFlowCS", "Logs");
            
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logFilePath = Path.Combine(logDirectory, $"Log_{DateTime.Now:yyyyMMdd}.txt");
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        /// <summary>
        /// 记录异常日志
        /// </summary>
        public void LogException(Exception ex, string message = "")
        {
            string exceptionMessage = $"{message} Exception: {ex.Message}";
            if (ex.StackTrace != null)
            {
                exceptionMessage += $"\nStackTrace: {ex.StackTrace}";
            }
            
            WriteLog("EXCEPTION", exceptionMessage);
        }

        /// <summary>
        /// 写入日志到文件
        /// </summary>
        private void WriteLog(string level, string message)
        {
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
                
                // 同时输出到调试窗口
                Debug.WriteLine(logEntry);
                
                // 写入到文件
                using (StreamWriter writer = new StreamWriter(_logFilePath, true))
                {
                    writer.WriteLine(logEntry);
                }
            }
            catch
            {
                // 日志记录失败时，不要抛出异常
                Debug.WriteLine($"Failed to write log: {level} - {message}");
            }
        }
    }
} 