using System.Text.Json;

namespace ubikCore;

public class UbikUtil
{
    public class UbikLogger
    {
        private readonly int _level;// 日志等级
        private readonly bool _isSave;// 是否保存日志
        private readonly string _logSavePath = "./log"; // 日志保存路径

        private readonly object _fileLock = new object(); // 文件互斥访问

        // 日志等级
        private const int OffLevel = 0;
        private const int FatalLevel = 1;
        private const int ErrorLevel = 2;
        private const int WarnLevel = 3;
        private const int InfoLevel = 4; // 默认级别
        private const int DebugLevel = 5;

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
            if (_isSave)
            {
                var currentTime = DateTime.Now.ToString("_yyyy-MM-dd HH-mm-ss");
                 _logSavePath=savePath  + currentTime + ".log";
            }
        }

        // Debug 打印debug级别日志
        public void Debug(string v)
        {
            if (_level >= DebugLevel)
            {
                var logString = "Debug: " + v;
                Console.WriteLine(Green + logString + Reset);
                SaveLogToFile(logString);
            }
        }

        // Info 打印info级别日志
        public void Info(string v)
        {
            if (_level >= InfoLevel)
            {
                var logString = v;
                Console.WriteLine(logString);
                SaveLogToFile(logString);
            }
        }

        // Warn 打印warn级别日志
        public void Warn(string v)
        {
            if (_level >= WarnLevel)
            {
                var logString = "Warn: " + v;
                Console.WriteLine(Yellow + logString + Reset);
                SaveLogToFile(logString);
            }
        }

        // Error 打印error级别日志
        public void Error(Exception v)
        {
            if (_level >= ErrorLevel)
            {
                var logString = "Error: " + v.Message;
                Console.WriteLine(Orange + logString + Reset);
                SaveLogToFile(logString);
            }
        }

        // Fatal 打印fatal级别日志
        public void Fatal(Exception v)
        {
            if (_level >= FatalLevel)
            {
                var logString = "Fatal: " + v.Message;
                Console.WriteLine(Red + logString + Reset);
                SaveLogToFile(logString);
            }
        }

        // SaveLogToFile 保存日志到文件
        private void SaveLogToFile(string v)
        {
            if (_isSave)
            {
                lock (_fileLock)
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