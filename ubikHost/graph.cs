using System.Threading.Channels;

namespace ubikHost;

public class Graph{
    private static Dictionary<int,RuntimeNode> _runtimeNodes = new Dictionary<int, RuntimeNode>(); //运行时节点, key为节点ID，value为节点
    private static List<RuntimeNode> _enterNodes = new List<RuntimeNode>(); // 入口节点
    private  static readonly ReaderWriterLockSlim GraphLock = new ReaderWriterLockSlim(); // 图读写锁    
    
    private static UbikUtil.UbikLogger _logger = new UbikUtil.UbikLogger(UbikUtil.UbikLogger.DebugLevel, false, "./");
    
    //TODO 图类的构造函数
    
    //运行时节点
    //被挂载的节点
    public class RuntimeNode
    {
        //运行时节点ID
        public int Id { get; private set; }
        public int State = Ready; //节点状态

        private static int _nextId = 1;
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
            _logger.Debug("Init node to runtime node "+node.Name);
            Id = _nextId++;
            Node = node;
            
            //初始化节点点集
            //初始化输入点
            Points.Input = new Dictionary<string, Edge>();
            foreach (var input in node.Input)
            {
                Points.Input.Add(input.Name,new Edge(0,null));
            }
            
            //初始化输出点
            Points.Output = new Dictionary<string, List<Edge>>();
            foreach (var output in node.Output)
            {
                Points.Output.Add(output.Name,new List<Edge>());
            }
        }

        //运行时节点出入点
        // 连接节点
        //output point<->input point
        public class RuntimeNodePoints
        {
            public Dictionary<string, List<Edge>>
                Output= new Dictionary<string, List<Edge>>(); // 输出点，key为出点名称，value ID值为输入点所在节点ID的集合

            public Dictionary<string, Edge> Input = new Dictionary<string, Edge>(); // 输入点，key为入点名称，value ID值为输出点所在节点ID
        }
    }
    
    //AddNode 添加节点
    //将运行时节点挂载到图中
    public static void AddNode(RuntimeNode node)
    {
        _logger.Debug("Add node "+node.Node.Name+" to graph, runtime node id is "+node.Id);
        
        //添加前检测
        if (node.State!=RuntimeNode.Ready)
        {
            throw new UbikUtil.UbikException("Node "+node.Node.Name+" is not ready");
        }
        if (_runtimeNodes.ContainsKey(node.Id))
        {
            throw new UbikUtil.UbikException("Node "+node.Node.Name+"'s ID "+node.Id+" already exist in graph");
        }
        
        //写锁
        GraphLock.EnterWriteLock();
        try
        {
            _runtimeNodes.Add(node.Id,node);
            if (node.Node.IsBeginningNode)
            {
                _enterNodes.Add(node);
            }
        }
        finally
        {
            GraphLock.ExitWriteLock();
        }
        
        _logger.Debug("Add node "+node.Node.Name+" to graph success, runtime node id is "+node.Id);
    }
    
    //RemoveNode 删除节点
    //将运行时节点从图中卸载
    public static void RemoveNode(int nodeId)
    {
        _logger.Debug("Remove node "+nodeId+" from graph, runtime node id is "+nodeId);
        //写锁
        GraphLock.EnterWriteLock();
        try
        {
            //删除节点
            if (!_runtimeNodes.Remove(nodeId, out var node))
            {
                throw new UbikUtil.UbikException("Node "+nodeId+" not exist in graph");
            }
            if (node.Node.IsBeginningNode)
            {
                _enterNodes.Remove(node);
            }
            
            //删除边
            //删除入边关系
            foreach (var input in node.Points.Input)
            {
                if (input.Value.NodeId != 0)
                {
                    DeleteEdge(input.Value.NodeId,nodeId,input.Key);
                }
            }
            
            //删除出边关系
            foreach (var output in node.Points.Output)
            {
                foreach (var edge in output.Value)
                {
                    DeleteEdge(nodeId,edge.NodeId,output.Key);
                }
            }
        }
        finally
        {
            GraphLock.ExitWriteLock();
        }
        
        _logger.Debug("Remove node "+nodeId+" from graph success, runtime node id is "+nodeId);
    }
    
    //UpdateEdge 更新边
    //以出点节点ID,入点节点ID与连接属性作为入参
    // 1.入点还未建立连接，入点与出点建立连接。
    // 2.入点已与出点A建立连接，入点改为和出点B建立连接。
    // 故只用输入入点和出点的ID即可，以检查入点为主
    // 以出点A与入点C为例
    // 1.首先节点X检测出点A是否存在，节点Y检测入点C是否存在。若不存在，则报错。存在则继续执行。
    // 2.出点A检查自己是否已经与入点C建立连接。若已经建立，则直接返回（可发出警告信息）。
    // 3.入点C检查自己是否已经和其他出点建立连接。若有连接，则断开连接。
    // 4.出点A与入点C建立连接。
    public static void UpdateEdge(int producerNodeId,int consumerNodeId,string attribute)
    {
        _logger.Debug("Update edge from node "+producerNodeId+" to node "+consumerNodeId+" with attribute "+attribute);
        
        //检查合法性,获取连接对象
        CheckUpdateValid(producerNodeId,consumerNodeId, attribute, out var producerNode, out var consumerNode, out var hasConsumerNodeLinkedOtherNode);
            
        //写锁
        GraphLock.EnterWriteLock();
        try
        {
            //建立数据管道
            var ch=Channel.CreateUnbounded<Value>();
            //向生产者节点的出点添加出边，指向消费者节点
            producerNode.Points.Output[attribute].Add(new Edge(consumerNodeId,ch));
            //若消费者节点已经和其他节点连接，则断开连接
            if (hasConsumerNodeLinkedOtherNode)
            {
                //暂时释放写锁
                GraphLock.ExitWriteLock();
                //删除边关系
                DeleteEdge(consumerNode.Points.Input[attribute].NodeId,consumerNodeId,attribute);
                //重新获取写锁
                GraphLock.EnterWriteLock();
            }
            
            //更新消费者节点的入点，指向生产者节点
            consumerNode.Points.Input[attribute] = new Edge(producerNodeId,ch);
        }
        finally
        {
            GraphLock.ExitWriteLock();
        }
        
        _logger.Debug("Update edge from node "+producerNodeId+" to node "+consumerNodeId+" with attribute "+attribute+" success");
    }
        
    //检查更新边合法性
    private static void CheckUpdateValid(int producerNodeId,int consumerNodeId, string attribute,out RuntimeNode producerNode,out RuntimeNode consumerNode,out bool hasconsumerNodeLinkedOtherNode)
    {
        //读锁
        GraphLock.EnterReadLock();
        try
        {
            //检查生产者节点是否存在
            if (!_runtimeNodes.TryGetValue(producerNodeId, out var producerNodeTmp))
            {
                throw new UbikUtil.UbikException("Node "+producerNodeId+" not exist ");
            }
            //检查出点是否存在于生产者节点中
            if (!producerNodeTmp.Points.Output.TryGetValue(attribute, out var edges))
            {
                throw new UbikUtil.UbikException("Output point " + attribute + " not exist ");
            }
            //检查生产者节点是否已经连接了当前消费者节点
            if (edges.Any(e => e.NodeId == consumerNodeId))
            {
                throw new UbikUtil.UbikException("Output point already linked in node " + producerNodeTmp.Node.Name);
            }
                
            //检查消费者节点是否存在
            if (!_runtimeNodes.TryGetValue(consumerNodeId, out var consumerNodeTmp))
            {
                throw new UbikUtil.UbikException("Node "+consumerNodeId+" not exist ");
            }
            //检查入点是否存在于消费者节点中
            if (!consumerNodeTmp.Points.Input.TryGetValue(attribute, out var edge))
            {
                throw new UbikUtil.UbikException("Input point "+attribute+" not exist in node " + consumerNodeTmp.Node.Name);
            }
                
            //检查入点是否已经和其他(包括生产者节点)节点连接
            hasconsumerNodeLinkedOtherNode = edge.NodeId != 0;
                
            producerNode = producerNodeTmp;
            consumerNode = consumerNodeTmp;
        }
        finally
        {
            GraphLock.ExitReadLock();
        }
    }
    
    // DeleteEdge 删除节点与节点之间的边
    // 1.首先节点X检测出点A是否存在，节点Y检测入点C是否存在。若不存在，则报错。存在则继续执行。
    // 2.C点检查自己是否与A点存在关系，A点检查自己是否与C点存在关系。若无均报错。
    // 3.C点清除和节点X的绑定，A点删除和节点Y的关系。
    public static void DeleteEdge(int producerNodeId,int consumerNodeId,string attribute)
    {
        _logger.Debug("Delete edge from node "+producerNodeId+" to node "+consumerNodeId+" with attribute "+attribute);
        
        CheckDeleteEdge(producerNodeId,consumerNodeId, attribute, out var producerNode, out var consumerNode);
        
        //写锁
        GraphLock.EnterWriteLock();
        try
        {
            //删除生产者节点中的边
            producerNode.Points.Output[attribute].RemoveAll(e => e.NodeId == consumerNodeId);
            
            //重置消费者节点中的边
            consumerNode.Points.Input[attribute] = new Edge(0,null);
        }
        finally
        {
            GraphLock.ExitWriteLock();
        }
        
        _logger.Debug("Delete edge from node "+producerNodeId+" to node "+consumerNodeId+" with attribute "+attribute+" success");
    }

    private static void CheckDeleteEdge(int producerNodeId,int consumerNodeId,string attribute,out RuntimeNode producerNode,out RuntimeNode consumerNode)
    {
        //读锁
        GraphLock.EnterReadLock();
        try
        {
            //检查生产者节点是否存在
            if (!_runtimeNodes.TryGetValue(producerNodeId, out var producerNodeTmp))
            {
                throw new UbikUtil.UbikException("Node "+producerNodeId+" not exist ");
            }
            //检查出点是否存在于生产者节点中
            if (!producerNodeTmp.Points.Output.TryGetValue(attribute, out var edges))
            {
                throw new UbikUtil.UbikException("Output point " + attribute + " not exist ");
            }
            //检查生产者节点是否已经连接了当前消费者节点
            if (edges.All(e => e.NodeId != consumerNodeId))
            {
                throw new UbikUtil.UbikException("Output point not linked in node " + producerNodeTmp.Node.Name);
            }
                
            //检查消费者节点是否存在
            if (!_runtimeNodes.TryGetValue(consumerNodeId, out var consumerNodeTmp))
            {
                throw new UbikUtil.UbikException("Node "+consumerNodeId+" not exist ");
            }
            //检查入点是否存在于消费者节点中
            if (!consumerNodeTmp.Points.Input.TryGetValue(attribute, out var edge))
            {
                throw new UbikUtil.UbikException("Input point " + attribute + " not exist in node " +
                                                 consumerNodeTmp.Node.Name);
            }
            //检查入点是否已经生产者节点连接
            if (edge.NodeId != producerNodeId)
            {
                throw new UbikUtil.UbikException("Input point not linked in node " + consumerNodeTmp.Node.Name);
            }
                
            producerNode = producerNodeTmp;
            consumerNode = consumerNodeTmp;
        }
        finally
        {
            GraphLock.ExitReadLock();
        }
    }
}