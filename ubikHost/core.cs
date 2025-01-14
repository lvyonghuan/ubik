﻿using Newtonsoft.Json;
using ubique.util;

namespace ubikHost;

public class Core
{
    private static Dictionary<string, Node> _nodes = new Dictionary<string, Node>();
    public Graph Graph = new Graph(Logger);
    private Config _config = new Config();

    public static UbikLogger Logger { get; private set; }

    private const string PluginPath = "./plugins";

    private const string TestPathPrefix = "../../../";

    public Core(string configPath, bool isInTest = false)
    {
        _config.ReadConfig(configPath);
        Logger = new UbikLogger(_config.log.LogLevel, _config.log.IsSaveLog, _config.log.LogSavePath);
        Graph= new Graph(Logger);

        //加载插件
        if (!isInTest)
        {
            Load.LoadPluginNodes(PluginPath);
        }
        else
        {
            Logger.Debug("Start loading plugin nodes");
            Load.LoadPluginNodes(TestPathPrefix + PluginPath);
        }
    }

    private class Config
    {
        public Log log = new Log();

        public class Log
        {
            public int LogLevel { get; set; } = UbikLogger.InfoLevel;
            public bool IsSaveLog { get; set; } = false;
            public string LogSavePath { get; set; } = "./";
        }

        public void ReadConfig(string filePath)
        {
            var jsonString = File.ReadAllText(filePath);
            var config = new Config();
            try
            {
                config = JsonConvert.DeserializeObject<Config>(jsonString) ?? throw new Exception("read config error");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("program will exit after 10s");
                Thread.Sleep(10000);
                Environment.Exit(0);
            }

            log.LogLevel = config.log.LogLevel;
            log.IsSaveLog = config.log.IsSaveLog;
            log.LogSavePath = config.log.LogSavePath;
        }
    }

    private class Load
    {
        public static void LoadPluginNodes(string pluginPath)
        {
            Logger.Debug("Start loading plugin nodes");
            //访问目录，获取目录下的所有文件夹
            var pluginDirs = Directory.GetDirectories(pluginPath);

            //遍历文件夹，访问文件夹下的info.json文件
            foreach (var pluginDir in pluginDirs)
            {
                Logger.Debug("Start loading plugin " + Path.GetFileName(pluginDir));
                var infoPath = Path.Combine(pluginDir, "info.json");
                var info = File.ReadAllText(infoPath);

                var pluginInfo = JsonConvert.DeserializeObject<Plugin>(info);
                if (pluginInfo == null)
                {
                    Logger.Error(new UbikException("load plugin " + Path.GetFileName(pluginDir) +
                                                            " info error, info is null, and it should not be null"));
                    continue;
                }

                foreach (var node in pluginInfo.nodes)
                {
                    if (node == null)
                    {
                        Logger.Error(new UbikException("load plugin " + Path.GetFileName(pluginDir) +
                                                                " info error, one of node is null, and it should not be null"));
                        continue;
                    }

                    if (string.IsNullOrEmpty(node.Name))
                    {
                        Logger.Error(new UbikException("load plugin " + Path.GetFileName(pluginDir) +
                                                                " info error, one of node name is null or empty, and it should not be null or empty"));
                        continue;
                    }

                    Logger.Debug("Start loading node " + node.Name);
                    if (!_nodes.TryAdd(node.Name, node))
                    {
                        Logger.Error(new UbikException("load plugin " + Path.GetFileName(pluginDir) +
                                                                " info error, node name " + node.Name +
                                                                " is already exist, and it should not be exist"));
                    }
                }
            }

            Logger.Debug("Loading plugin nodes success");
        }

        private class Plugin
        {
            public string Name { get; set; } = "";
            public string Version { get; set; } = "";
            public String Description { get; set; } = "";
            public string Author { get; set; } = "";
            public bool netCall { get; set; } = false;
            public List<Node> nodes { get; set; } = new List<Node>();
        }
    }
}
