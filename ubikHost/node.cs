using ubique.plugin;

namespace ubikHost;

//节点类型
//等待被转载为运行时节点
public class Node(string name, string info, bool isBeginningNode,bool needNetCall, List<Value> input,List<Value> output, List<Value> @params,Plugin plugin)
{
    private Plugin _plugin = plugin;
    public string Name { get; private set; } = name;
    public string Info { get; private set; } = info;
    public bool IsBeginningNode { get; private set; } = isBeginningNode;
    public bool NeedNetCall { get; private set; } = needNetCall;
    
    // 节点要求的输入
    public List<Value> Input { get; set; } = input;

    // 节点的输出
    public List<Value> Output { get; set; } = output;

    // 节点提供给用户自定义的参数
    public List<Value> Params { get; set; } = @params;

    public int AddRuntimeNode()
    {
        var runtimeNode=new Graph.RuntimeNode(this);
        Graph.AddNode(runtimeNode);
        return runtimeNode.Id;
    }

    public void AddNode(int id,Node node)
    {
        _plugin.AddNode(id,node);
    }

    public void RemoveNode(int id)
    {
        _plugin.RemoveNode(id);
    }
    
    public void SetPlugin(Plugin plugin)
    {
        _plugin = plugin;
    }

    public void SetCommunicator(int id,Communicator communicator)
    {
        _plugin.AddCommunicationToNode(id,communicator);
    }
    
    public void Run(int id)
    {
        _plugin.Run(id);
    }
}

// 边
// 边是逻辑形式的连接
public class Edge(int nodeId, Value value,string pointToPointName)
{
    public int NodeId { get; private set; } = nodeId;
    public Value Value { get; private set; }=value;
    public ConsumerBuffer Buffer { get; set; } = new ConsumerBuffer();
    public string PointToPointName = pointToPointName;
}

public class Value(string attribute, string name, string type, object value)
{
    public string Attribute { get; private set; } = attribute; // 参数属性——属性相同即可建立边关系
    public string Name { get; private set; } = name; // 参数名称
    public string Type { get; private set; } = type; // 参数数值类型
    public object Val { get; private set; } = value; // 参数值
}