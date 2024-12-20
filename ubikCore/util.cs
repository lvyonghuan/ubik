using System;
using System.IO;
using System.Text.Json;

namespace ubikCore;

public class UbikUtil
{
    public class UbikLogger
    {
        private static  int _level;// 日志等级
        private static  bool _isSave;// 是否保存日志
        private static  string _logSavePath = "./log"; // 日志保存路径

        private static readonly object FileLock = new object(); // 文件互斥访问

        // 日志等级
        public const int OffLevel = 0;
        public const int FatalLevel = 1;
        public const int ErrorLevel = 2;
        public const int WarnLevel = 3;
        public const int InfoLevel = 4; // 默认级别
        public const int DebugLevel = 5;

        // 日志颜色
        private const string Reset = "\033[0m";
        private const string Red = "\033[31m";
        private const string Orange = "\033[33m";
        private const string Yellow = "\033[93m";
        private const string Green = "\033[32m";

        // 启动时日志库初始化
        public UbikLogger(int level,bool isSave,string savePath)
        {
            _level = level;
            _isSave = isSave;
            if (!_isSave) return;
            var currentTime = DateTime.Now.ToString("_yyyy-MM-dd HH-mm-ss");
            _logSavePath = savePath + currentTime + ".log";
        }

        // Debug 打印debug级别日志
        public void Debug(string v)
        {
            if (_level < DebugLevel) return;
            var logString = "Debug: " + v;
            Console.WriteLine(Green + logString + Reset);
            SaveLogToFile(logString);
        }

        // Info 打印info级别日志
        public void Info(string v)
        {
            if (_level < InfoLevel) return;
            Console.WriteLine(v);
            SaveLogToFile(v);
        }

        // Warn 打印warn级别日志
        public void Warn(string v)
        {
            if (_level < WarnLevel) return;
            var logString = "Warn: " + v;
            Console.WriteLine(Yellow + logString + Reset);
            SaveLogToFile(logString);
        }

        // Error 打印error级别日志
        public void Error(Exception v)
        {
            if (_level < ErrorLevel) return;
            var logString = "Error: " + v.Message;
            Console.WriteLine(Orange + logString + Reset);
            SaveLogToFile(logString);
        }

        // Fatal 打印fatal级别日志
        public void Fatal(Exception v)
        {
            if (_level < FatalLevel) return;
            var logString = "Fatal: " + v.Message;
            Console.WriteLine(Red + logString + Reset);
            SaveLogToFile(logString);
        }

        // SaveLogToFile 保存日志到文件
        private void SaveLogToFile(string v)
        {
            if (!_isSave) return;
            lock (FileLock)
            {
                try
                {
                    using (var file = new StreamWriter(_logSavePath, true))
                    {
                        file.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":" + v);
                    }
                }
                catch (Exception ex)
                {
                    // 唯二不调用error级别却打印error日志的地方
                    Warn("Write log to file failed: " + ex.Message);
                }
            }
        }
    }
    
    public class UbikException(string message) : Exception(message)
    {
        public string ErrorMessage { get; private set; } = message;
        public DateTime OccurredAt { get; private set; } = DateTime.Now;
        public string StackInfo { get; private set; } = Environment.StackTrace;

        public override string ToString() =>
            $"{ErrorMessage}\nOccurred at: {OccurredAt}\nStack trace:\n{StackInfo}";
    }
}