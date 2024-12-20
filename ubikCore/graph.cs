using System.Threading.Channels;

namespace ubikCore;

public class Graph{
    private static Dictionary<int,RuntimeNode> _runtimeNodes = new Dictionary<int, RuntimeNode>(); //运行时节点, key为节点ID，value为节点
    private static List<RuntimeNode> _enterNodes = new List<RuntimeNode>(); // 入口节点
    private  static readonly ReaderWriterLockSlim GraphLock = new ReaderWriterLockSlim(); // 图读写锁    
    
    //运行时节点
    //被转载的节点
    public class RuntimeNode
    {
        //运行时节点ID
        public int Id { get; private set; }
        public int State = Ready; //节点状态

        private static int _nextId = 0;
        public Node Node { get;private set;} //节点
        public bool HasVisited = false; //是否已经被访问过，用于图的合法性检查

        //节点状态定义
        public const int Running = 0;
        public const int Ready = 1;
        public const int Failed = 2;
        
        //节点点集
        public RuntimeNodePoints Points = new RuntimeNodePoints();

        public RuntimeNode(Node node)
        {
            Id = _nextId++;
            Node = node;
        }

        //运行时节点出入点
        // 连接节点
        //output point<->input point
        public class RuntimeNodePoints
        {
            public Dictionary<string, List<Edge>>
                Output= new Dictionary<string, List<Edge>>(); // 输出点，key为参数名称，value ID值为输入点所在节点ID的集合

            public Dictionary<string, Edge> Input = new Dictionary<string, Edge>(); // 输入点，key为参数名称，value ID值为输出点所在节点ID
        }
    }
    
    //更新边
    //以出点节点ID,入点节点ID与连接属性作为入参
    // 1.入点还未建立连接，入点与出点建立连接。
    // 2.入点已与出点A建立连接，入点改为和出点B建立连接。
    // 故只用输入入点和出点的ID即可，以检查入点为主
    // 以出点A与入点C为例
    // 1.首先节点X检测出点A是否存在，节点Y检测入点C是否存在。若不存在，则报错。存在则继续执行。
    // 2.出点A检查自己是否已经与入点C建立连接。若已经建立，则直接返回（可发出警告信息）。入点C同理。
    // 3.入点C检查自己是否已经和其他出点建立连接。若有连接，则断开连接。
    // 4.出点A与入点C建立连接。
    public void UpdateEdge(int opNodeId,int linkNodeId,string attribute)
    {
        //检查合法性,获取连接对象
        CheckUpdateValid(opNodeId,linkNodeId, attribute, out var opNode, out var linkNode);
            
        //写锁
        GraphLock.EnterWriteLock();
        try
        {
            //建立数据管道
            var ch=Channel.CreateUnbounded<Value>();
            //向操作节点的出点添加出边，指向连接节点
            opNode.Points.Output[attribute].Add(new Edge(linkNodeId,ch));
            //向连接节点的入点添加入边，指向操作节点，接收来自操作节点的管道
            linkNode.Points.Input[attribute] = new Edge(opNodeId,ch);
        }
        finally
        {
            GraphLock.ExitWriteLock();
        }
    }
        
    //检查添加出边合法性
    private void CheckUpdateValid(int opNodeId,int linkNodeId, string attribute,out RuntimeNode opNode,out RuntimeNode linkNode)
    {
        //读锁
        GraphLock.EnterReadLock();
        try
        {
            //检查操作节点是否存在
            if (!_runtimeNodes.TryGetValue(opNodeId, out var opNodeTmp))
            {
                throw new UbikUtil.UbikException("Node "+opNodeId+" not exist ");
            }
            //检查出点是否存在于操作节点中
            if (!opNodeTmp.Points.Output.TryGetValue(attribute, out var edges))
            {
                throw new UbikUtil.UbikException("Output point "+attribute+" not exist ");
            }
                
            //检查操作节点是否已经连接了当前连接节点
            if (edges.Any(e => e.NodeId == linkNodeId))
            {
                throw new UbikUtil.UbikException("Output point already linked in node " + opNodeTmp.Node.Name);
            }
                
            //检查连接节点是否存在
            if (!_runtimeNodes.TryGetValue(linkNodeId, out var linkNodeTmp))
            {
                throw new UbikUtil.UbikException("Node "+linkNodeId+" not exist ");
            }
            //检查入点是否存在于连接节点中
            if (!linkNodeTmp.Points.Input.TryGetValue(attribute, out var edge))
            {
                throw new UbikUtil.UbikException("Input point "+attribute+" not exist in node " + linkNodeTmp.Node.Name);
            }
                
            //检查入点是否已经和其他(包括操作节点)节点连接
            if (edge.NodeId!=0)
            {
                throw new UbikUtil.UbikException("Input point already linked in node " + linkNodeTmp.Node.Name);
            }
                
            opNode = opNodeTmp;
            linkNode = linkNodeTmp;
        }
        finally
        {
            GraphLock.ExitReadLock();
        }
    }
}