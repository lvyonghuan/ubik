using System.Collections.Generic;
using System.Threading.Channels;

namespace ubikCore;

//节点类型
//等待被转载为运行时节点
public class Node(string name, string info, bool isBeginningNode, Dictionary<string, string> input, Dictionary<string, string> output, Dictionary<string, string> @params)
{
    public string Name { get; private set; } = name;
    public string Info { get; private set; } = info;
    public bool IsBeginningNode { get; private set; } = isBeginningNode;
    // 节点要求的输入
    // key为参数名称
    // value为参数属性
    public Dictionary<string, string> Input { get; set; } = input;

    // 节点的输出
    // key为参数名称
    // value为参数属性
    public Dictionary<string, string> Output { get; set; } = output;

    // 节点提供给用户自定义的参数
    // key为参数名称
    // value为参数属性
    public Dictionary<string, string> Params { get; set; } = @params;

    public int AddNode()
    {
        var runtimeNode=new Graph.RuntimeNode(this);
        Graph.AddNode(runtimeNode);
        return runtimeNode.Id;
    }
}

// 边
public class Edge(int id, Channel<Value> ch)
{
    public int NodeId  = id; // 对应节点ID
    public Channel<Value> Ch { get; private set; } = ch; // 边的通道。由出边生产，入边消费。
}

public class Value(string attribute, string name, string type, object value)
{
    public string Attribute { get; private set; } = attribute; // 参数属性——属性相同即可建立边关系
    public string Name { get; private set; } = name; // 参数名称
    public string Type { get; private set; } = type; // 参数数值类型
    public object Val { get; private set; } = value; // 参数值
}