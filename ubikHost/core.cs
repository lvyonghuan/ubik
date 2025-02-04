using Newtonsoft.Json;
using ubikHost.api;
using ubique.util;

namespace ubikHost;

public class Core
{
    private static Dictionary<string,Plugin> _plugins = new Dictionary<string, Plugin>();
    private static Dictionary<string,Plugin> _MountPlugins = new Dictionary<string, Plugin>();
    private static Dictionary<string, Node> _nodes = new Dictionary<string, Node>();
    public Graph Graph = new Graph();
    private Router _router;
    private readonly Config _config = new Config();

    public static UbikLogger Logger { get; private set; }
    public static OpSys OpSysType { get; private set; }

    private const string PluginPath = "./plugins";

    private const string TestPathPrefix = "../../../";

    public Core(string configPath, bool isInTest = false)
    {
        OpSysType = JudgeOpSys();
        
        _config.ReadConfig(configPath);
        Logger = new UbikLogger(_config.log.LogLevel, _config.log.IsSaveLog, _config.log.LogSavePath);

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
        
        //初始化网络API
        _router = new Router(this);
    }

    //启动网络API
    public void StartRouter()
    {
        try
        {
            _router.Init();
        }
        catch(Exception e)
        {
            Logger.Fatal(e);
        }
    }

    public int AddNode(string nodeName)
    {
        if (!_nodes.TryGetValue(nodeName, out var node))
        {
            throw new UbikException("node " + nodeName + " not found");
        }

        return node.AddRuntimeNode();
    }
    
    public void RemoveNode(int nodeId)
    {
        Graph.RemoveNode(nodeId);
    }
    
    public void UpdateEdge(int producerNodeId,int consumerNodeId,string producerPointName,string consumerPointName)
    {
        Graph.UpdateEdge(producerNodeId,consumerNodeId,producerPointName,consumerPointName);
    }
    
    public void DeleteEdge(int producerNodeId,int consumerNodeId,string producerPointName,string consumerPointName)
    {
        Graph.DeleteEdge(producerNodeId,consumerNodeId,producerPointName,consumerPointName);
    }
    
    public UbikException BeforeRunSet()
    {
        try
        {
            //挂载所有插件
            foreach (var plugin in _plugins)
            {
                Logger.Debug("Mounting plugin " + plugin.Key);
                var isMount=plugin.Value.Mount();
                if (isMount)
                {
                    _MountPlugins.TryAdd(plugin.Key, plugin.Value);
                }
            }
            
            //将逻辑节点加入插件
            foreach (var plugin in _MountPlugins)
            {
                plugin.Value.AddNodeToPlugin();
            }
            
            Graph.BeforeRunSet();
        }
        catch (UbikException e)
        {
            return e;
        }
        catch (Exception e)
        {
            return new UbikException(e.Message);
        }
        
        return null;
    }
    
    public void Run()
    {
        Graph.Run();
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
                
                if (!_plugins.TryAdd(pluginInfo.Name, pluginInfo))
                {
                    Logger.Error(new UbikException("load plugin " + Path.GetFileName(pluginDir) +
                                                            " info error, plugin name " + pluginInfo.Name +
                                                            " is already exist, and it should not be exist"));
                    continue;
                }

                foreach (var node in pluginInfo.nodes)
                {

                    if (string.IsNullOrEmpty(node.Name))
                    {
                        Logger.Error(new UbikException("load plugin " + Path.GetFileName(pluginDir) +
                                                                " info error, one of node name is null or empty, and it should not be null or empty"));
                        continue;
                    }

                    Logger.Debug("Start loading node " + node.Name);
                    node.SetPlugin(pluginInfo);
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
    }
    
    //判断操作系统
    private OpSys JudgeOpSys()
    {
        var sys = Environment.OSVersion.Platform;
        switch (sys)
        {
            case PlatformID.Win32NT:
                return OpSys.Windows;
            case PlatformID.Unix:
                return OpSys.Linux;
            default:
                throw new UbikException("Unsupported operating system"+sys);
            // return (int) OpSys.Other;
        }
    }
}

public enum OpSys
{
    Windows,
    Linux,
    Other
}