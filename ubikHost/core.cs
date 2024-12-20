using System.Text.Json;

namespace ubikHost;

public class Core
{
    private Dictionary<string, Node> _nodes = new Dictionary<string, Node>();
    public Graph Graph = new Graph();
    private Config _config = new Config();
    
    public UbikUtil.UbikLogger Logger { get; private set; }
    
    public Core(string configPath)
    {
        _config.ReadConfig(configPath);
        Logger = new UbikUtil.UbikLogger(_config.LogLevel, _config.IsSaveLog, _config.LogSavePath);
    }
    
    private class Config
    {
        public int LogLevel { get; private set; }
        public bool IsSaveLog { get; private set; }
        public string LogSavePath { get; private set; }= "./";

        public void ReadConfig(string filePath)
        {
            var jsonString = File.ReadAllText(filePath);
            var config = new Config();
            try
            {
                config = JsonSerializer.Deserialize<Config>(jsonString) ?? throw new Exception("read config error");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("program will exit after 10s");
                Thread.Sleep(10000);
                Environment.Exit(0);
            }
            
            LogLevel = config.LogLevel;
            IsSaveLog = config.IsSaveLog;
            LogSavePath = config.LogSavePath;
        }
    }
}