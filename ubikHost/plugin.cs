using System.Runtime.InteropServices;
using ubique.util;
using ubique.plugin;

namespace ubikHost;

public class Plugin
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public String Description { get; set; } = "";
    public string Author { get; set; } = "";
    public bool NetCall { get; set; } = false;
    public List<Node> nodes { get; set; } = new List<Node>();

    private int _count = 0;
    private IPlugin _plugin;

    private const string PluginPath = "./plugins/";

    public void Mount()
    {
        var opSys = Core.OpSysType;
        //如果count>0
        //表示该plugin挂载在图上的节点数量大于0
        //需要进行物理挂载
        if (_count > 0)
        {
            {
                if (NetCall)
                {
                    //TODO:进行网络挂载
                }
                else //通过动态链接挂载
                {
                    DlMount(opSys);
                }
            }
        }
    }

    public void AddNode()
    {
        _count++;
    }

    public void RemoveNode()
    {
        _count--;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IPlugin Init();

    //使用脚本文件启动网络挂载程序
    private const string WindowsNetMountSuffix = ".bat";
    private const string LinuxNetMountSuffix = ".sh";

    private void NetMount(OpSys opSys)
    {
        //TODO:网络挂载
    }

    //使用动态链接库启动动态挂载程序
    private const string WindowsDlMountSuffix = ".dll";
    private const string LinuxDlMountSuffix = ".so";

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

        var path = PluginPath + Name + "/" + Name + suffix;
        //TODO:动态链接挂载

        if (!File.Exists(path))
        {
            throw new UbikException("plugin " + Name + " not found");
        }

        IntPtr handle = NativeLibrary.Load(path);

        try
        {
            var ptr = NativeLibrary.GetExport(handle, "Init");
            var init = Marshal.GetDelegateForFunctionPointer<Init>(ptr);
            _plugin = init();
        }
        catch (Exception e)
        {
            throw new UbikException("plugin " + Name + " init error: " + e);
        }
    }
}