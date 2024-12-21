using System.Threading.Channels;

namespace ubikHost;

//节点类型
//等待被转载为运行时节点
public class Node(string name, string info, bool isBeginningNode, List<Value> input,List<Value> output, List<Value> @params)
{
    public string Name { get; private set; } = name;
    public string Info { get; private set; } = info;
    public bool IsBeginningNode { get; private set; } = isBeginningNode;
    
    //FIXME 全部改成list
    
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
}

// 边
public class Edge(int nodeId,Value value)
{
    public int NodeId { get; private set; } = nodeId;
    public Value Vlaue { get; private set; } = value;
    
    public Transport NewTransport()
    {
        return new Transport();
    }
    
    public class Transport
    {
        public object Value = new object();
        public readonly Mutex Mutex = new Mutex();
    }
}

public class Value(string attribute, string name, string type, object value)
{
    public string Attribute { get; private set; } = attribute; // 参数属性——属性相同即可建立边关系
    public string Name { get; private set; } = name; // 参数名称
    public string Type { get; private set; } = type; // 参数数值类型
    public object Val { get; private set; } = value; // 参数值
}