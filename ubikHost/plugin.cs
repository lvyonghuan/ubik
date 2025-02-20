using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework.Internal;
using ubique.util;
using ubique.plugin;

namespace ubikHost;

//插件类
//插件是节点的集合
public class Plugin
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public String Description { get; set; } = "";
    public string Author { get; set; } = "";
    public bool NetCall { get; set; } = false;
    public List<Node> nodes { get; set; } = new List<Node>();
    private Dictionary<int,Node> _runtimeNodes=new Dictionary<int, Node>();
    private Dictionary<int,IRuntimeNode> _iRuntimeNodes=new Dictionary<int, IRuntimeNode>();

    private int _count = 0;
    private IPlugin _plugin;

    private const string PluginPath = @"\plugins\";
    private string _currentPath = Directory.GetCurrentDirectory();

    //挂载插件，返回是否被挂载
    public bool Mount()
    {
        var opSys = Core.OpSysType;
        //如果count>0
        //表示该plugin挂载在图上的节点数量大于0
        //需要进行物理挂载
        Core.Logger.Debug("mount plugin " + Name+", count:"+_count);
        if (_count > 0)
        {
           if (NetCall)
           {
                NetMount(opSys);
           }
           else //通过动态链接挂载
           {
               DlMount(opSys);
           }
           Core.Logger.Debug("plugin " + Name + " mounted");
           return true;
        }
        return false;
    }

    //TODO update because remove node
    public void AddNodeToPlugin()
    {
        if(_plugin==null)
        {
            throw new UbikException("plugin " + Name + " not mounted");
        }
        
        foreach (var pair in _runtimeNodes)
        {
            if (!_plugin.AddRuntimeNode(pair.Value.Name,pair.Key,out var iRuntimeNode))
            {
                throw new UbikException("add node " + pair.Value.Name + " to plugin " + Name + " failed");
            }

            if(!_iRuntimeNodes.TryAdd(pair.Key,iRuntimeNode))
            {
                throw new UbikException("add node " + pair.Value.Name + " to plugin " + Name + " failed");
            }
        }
    }
    
    public void AddCommunicationToNode(int id,Communicator communicator)
    {
        if (!_iRuntimeNodes.TryGetValue(id, out var iRuntimeNode))
        {
            throw new UbikException("node " + id + " not found in plugin " + Name);
        }
        if (!iRuntimeNode.GetCommunicator(communicator))
        {
            throw new UbikException("node " + id + " get communicator failed");
        }
    }

    public void AddNode(int id,Node node)
    {
        if (!_runtimeNodes.TryAdd(id, node))
        {
            throw new UbikException("node " + id + " already exists in plugin " + Name);
        }
        _count++;
    }

    public void RemoveNode(int id)
    {
        if (!_runtimeNodes.Remove(id))
        {
            throw new UbikException("node " + id + " not found in plugin " + Name);
        }
        _count--;
    }

    public void Run(int id)
    {
        if (!_iRuntimeNodes.TryGetValue(id, out var iRuntimeNode))
        {
            throw new UbikException("node " + id + " not found in plugin " + Name);
        }
        if (!iRuntimeNode.Run())
        {
            throw new UbikException("node " + id + " run failed");
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IPlugin Init();

    //使用脚本文件启动网络挂载程序
    private const string WindowsNetMountSuffix = ".bat";
    private const string LinuxNetMountSuffix = ".sh";

    private void NetMount(OpSys opSys)
    {
        //根据系统决定执行脚本的后缀名
        var suffix = "";
        switch (opSys)
        {
            case OpSys.Windows:
                suffix = WindowsNetMountSuffix;
                break;
            case OpSys.Linux:
                suffix = LinuxNetMountSuffix;
                break;
            default:
                throw new UbikException("unknown os");
        }
        
        //拼接脚本文件路径
        var path = _currentPath+PluginPath + Name + @"\" + Name + suffix;
        if (!File.Exists(path))
        {
            throw new UbikException("plugin " + Name + " not found");
        }
        
        //执行脚本文件
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = path,
                RedirectStandardOutput = true,
                UseShellExecute = true,
                CreateNoWindow = false
            }
        };

        try
        {
            process.Start();
        }
        catch (Exception e)
        {
            throw new UbikException("plugin " + Name + " net mount error: " + e);
        }
    }

    //使用动态链接库启动动态挂载程序
    private const string WindowsDlMountSuffix = ".dll";
    private const string LinuxDlMountSuffix = ".so";

    //FIXME 多个插件使用一个库，只挂载一次应该即可
    private void DlMount(OpSys opSys)
    {
        var suffix = "";
        switch (opSys)
        {
            case OpSys.Windows:
                suffix = WindowsDlMountSuffix;
                break;
            case OpSys.Linux:
                suffix = LinuxDlMountSuffix;
                break;
            default:
                throw new UbikException("unknown os");
        }

        var path = _currentPath+PluginPath + Name + @"\" + Name + suffix;
        if (!File.Exists(path))
        {
            throw new UbikException("plugin " + Name + " not found");
        }
        
        try
        {
            Assembly pluginAssembly = Assembly.LoadFile(path);
            if (pluginAssembly == null)
            {
                throw new UbikException("plugin " + Name + " load failed");
            }
            
            var pluginType = pluginAssembly.GetType( Name + "."+Name);
            if (pluginType == null)
            {
                Core.Logger.Debug(Name + "."+Name);
                throw new UbikException("plugin " + Name + " type not found");
            }
            
            var method = pluginType.GetMethod("Init");
            if (method == null)
            {
                throw new UbikException("plugin " + Name + " init method not found");
            }
            
            var init = (Init) Delegate.CreateDelegate(typeof(Init), method);
            if (init == null)
            {
                throw new UbikException("plugin " + Name + " init delegate create failed");
            }
            
            _plugin = init();
        }
        catch (Exception e)
        {
            throw new UbikException("plugin " + Name + " init error: " + e);
        }
    }
    
    private class Plugins
    {
        //TODO 使用管道进行阻塞通知
        public static Dictionary<string,CancellationToken> MountingPlugins=new Dictionary<string, CancellationToken>();//正在挂载的插件的集合
        public static Dictionary<string,IPlugin> MountedPlugins=new Dictionary<string, IPlugin>();//已经挂载的插件的集合
    }
}
