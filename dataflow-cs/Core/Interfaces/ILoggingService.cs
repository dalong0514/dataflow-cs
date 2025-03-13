using System;

namespace dataflow_cs.Core.Interfaces
{
    /// <summary>
    /// 日志服务接口，定义日志记录方法
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void LogInfo(string message);

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">警告消息</param>
        void LogWarning(string message);

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">错误消息</param>
        void LogError(string message);

        /// <summary>
        /// 记录异常日志
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="message">额外信息</param>
        void LogException(Exception ex, string message = "");
    }
} 