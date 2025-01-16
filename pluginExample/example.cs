namespace PluginExample;

using ubique.plugin;

public class PluginExample
{
    public static IPlugin Init()
    {
        return new Plugin();
    }
}

public class Plugin : IPlugin
{
    private Dictionary<int,IRuntimeNode> _nodes = new Dictionary<int, IRuntimeNode>();
    
    public bool State()
    {
        return true;
    }

    public bool AddRuntimeNode(string nodeName,int runtimeNodeId ,out IRuntimeNode iRuntimeNode)
    {
        switch (nodeName)
        {
            case "Init":
                IRuntimeNode init = new nodes.Init();
                _nodes.Add(runtimeNodeId,init);
                iRuntimeNode=init;
                return true;
            case "Increasing":
                IRuntimeNode increasing = new nodes.Increasing();
                _nodes.Add(runtimeNodeId,increasing);
                iRuntimeNode=increasing;
                return true;
            case "Add_Print":
                IRuntimeNode addPrint = new nodes.AddPrint();
                _nodes.Add(runtimeNodeId,addPrint);
                iRuntimeNode=addPrint;
                return true;
            default:
                iRuntimeNode = null;
                return false;
        }
    }
    
    public bool RemoveRuntimeNode(int runtimeNodeId)
    {
        
        return true;
    }
}