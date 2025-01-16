using System.Threading.Channels;
using ubique.plugin;
using ubique.util;

namespace ubikHost;

public class Graph{
    private static Dictionary<int,RuntimeNode> _runtimeNodes = new Dictionary<int, RuntimeNode>(); //运行时节点, key为节点ID，value为节点
    private static List<RuntimeNode> _enterNodes = new List<RuntimeNode>(); // 入口节点
    private  static readonly ReaderWriterLockSlim GraphLock = new ReaderWriterLockSlim(); // 图读写锁    

    public void BeforeRunSet()
    {
        //初始化运行时节点
        foreach (var node in _runtimeNodes.Values)
        {
            //获取节点的消费者缓冲区
            var consumerBuffers = new Dictionary<string, List<ConsumerBuffer>>();
            var consumerBuffer = new Dictionary<string, ConsumerBuffer>();
            
            foreach (var output in node.Points.Output)
            {
                consumerBuffers.Add(output.Key,output.Value.Select(e => e.Buffer).ToList());
            }
            foreach (var input in node.Points.Input)
            {
                consumerBuffer.Add(input.Key,input.Value.Buffer);
            }
            
            node.InitRuntimeNode(consumerBuffers, consumerBuffer);
        }
    }

    public void Run()
    {
        foreach (var node in _runtimeNodes.Values)
        {
            node.Run();
        }
    }
    
    //运行时节点
    //被挂载的节点
    public class RuntimeNode
    {
        //运行时节点ID
        public int Id { get; private set; }
        public int State = Ready; //节点状态

        private static int _nextId = 1;
        public Node Node { get;private set;} //节点
        
        //节点状态定义
        public const int Running = 0;
        public const int Ready = 1;
        public const int Failed = 2;
        
        //节点点集
        public RuntimeNodePoints Points = new RuntimeNodePoints();
        
        //通讯器
        private Communicator _communicator;

        public RuntimeNode(Node node)
        {
            Core.Logger.Debug("Init node to runtime node "+node.Name);
            Id = _nextId++;
            Node = node;
            
            //初始化节点点集
            //初始化输入点
            Points.Input = new Dictionary<string, Edge>();
            foreach (var input in node.Input)
            {
                Points.Input.Add(input.Name,new Edge(0,input));
            }
            
            //初始化输出点
            Points.Output = new Dictionary<string, List<Edge>>();
            foreach (var output in node.Output)//FIXME
            {
                Points.Output.Add(output.Name,new List<Edge>());
            }
        }
        
        public void InitRuntimeNode(Dictionary<string,List<ConsumerBuffer>> consumerBuffers,Dictionary<string,ConsumerBuffer> consumerBuffer)
        {
            SetCommunicator();
            SetBuffer(consumerBuffers,consumerBuffer);
        }
        
        private void SetCommunicator()
        {
            if (Node.NeedNetCall)
            {
                _communicator = new CommunicatorGrpc(Id);
            }
            else
            {
                _communicator = new CommunicatorDL(Id);
            }
            
            Node.SetCommunicator(Id,_communicator);
        }
        
        private void SetBuffer(Dictionary<string,List<ConsumerBuffer>> consumerBuffers,Dictionary<string,ConsumerBuffer> consumerBuffer)
        {
            _communicator.SetBuffer(consumerBuffers,consumerBuffer);
        }
        
        public void Run()
        {
            Core.Logger.Debug("Run node "+Node.Name);
            State = Running;
            Node.Run(Id);
        }
        
        //测试接口//
        //仅供测试使用，生产环境请勿调用//
        public void SendMessage(string attribute,object message)
        {
            _communicator.Send(attribute,message);
        }
        
        //测试接口//
        //仅供测试使用，生产环境请勿调用//
        public async Task<object> ReceiveMessage(string attribute)
        {
            return await _communicator.Receive(attribute);
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
        Core.Logger.Debug("Add node "+node.Node.Name+" to graph, runtime node id is "+node.Id);
        
        //添加前检测
        if (node.State!=RuntimeNode.Ready)
        {
            throw new UbikException("Node "+node.Node.Name+" is not ready");
        }
        if (_runtimeNodes.ContainsKey(node.Id))
        {
            throw new UbikException("Node "+node.Node.Name+"'s ID "+node.Id+" already exist in graph");
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
            //plugin计数器加一
            node.Node.AddNode(node.Id,node.Node);
        }
        finally
        {
            GraphLock.ExitWriteLock();
        }
        
        Core.Logger.Debug("Add node "+node.Node.Name+" to graph success, runtime node id is "+node.Id);
    }
    
    //RemoveNode 删除节点
    //将运行时节点从图中卸载
    public static void RemoveNode(int nodeId)
    {
        Core.Logger.Debug("Remove node "+nodeId+" from graph, runtime node id is "+nodeId);
        //写锁
        GraphLock.EnterWriteLock();
        try
        {
            //删除节点
            if (!_runtimeNodes.Remove(nodeId, out var node))
            {
                throw new UbikException("Node "+nodeId+" not exist in graph");
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
                    DeleteEdge(input.Value.NodeId,nodeId,input.Key,node.Points.Input[input.Key].Value.Name);
                }
            }
            
            //删除出边关系
            foreach (var output in node.Points.Output)
            {
                foreach (var edge in output.Value)
                {
                    DeleteEdge(nodeId,edge.NodeId,output.Key,edge.Value.Name);
                }
            }
            //plugin计数器减一
            node.Node.RemoveNode(node.Id);
        }
        finally
        {
            GraphLock.ExitWriteLock();
        }
        
        Core.Logger.Debug("Remove node "+nodeId+" from graph success, runtime node id is "+nodeId);
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
    public static void UpdateEdge(int producerNodeId,int consumerNodeId,string producerNodePointName,string consumerNodePointName)
    {
        var producerNode = GetRuntimeNode(producerNodeId);
        var consumerNode = GetRuntimeNode(consumerNodeId);
        
            
        Core.Logger.Debug("Update edge from node "+producerNodeId+" to node "+consumerNodeId+" with point "+producerNodePointName);
        
        //检查合法性,获取连接对象
        CheckUpdateValid(producerNode,consumerNode, producerNodePointName,consumerNodePointName,out var producerNodePointValue,out var consumerNodePointValue, out var hasConsumerNodeLinkedOtherNode);
            
        //写锁
        GraphLock.EnterWriteLock();
        try
        {
            //建立生产者-消费者缓冲区
            var consumerBuffer = new ConsumerBuffer();
            //向生产者节点的出点添加出边，指向消费者节点
            producerNode.Points.Output[producerNodePointName].Add(new Edge(consumerNodeId, producerNodePointValue){ Buffer = consumerBuffer }); 
            //若消费者节点已经和其他节点连接，则断开连接
            if (hasConsumerNodeLinkedOtherNode)
            {
                //暂时释放写锁
                GraphLock.ExitWriteLock();
                //删除边关系
                var oldProducer = consumerNode.Points.Input[consumerNodePointName];
                DeleteEdge(oldProducer.NodeId,consumerNodeId,oldProducer.Value.Name,consumerNodePointName);
                //重新获取写锁
                GraphLock.EnterWriteLock();
            }
            
            //更新消费者节点的入点，指向生产者节点
            consumerNode.Points.Input[consumerNodePointName] = new Edge(producerNodeId, consumerNodePointValue){ Buffer = consumerBuffer };
        }
        finally
        {
            GraphLock.ExitWriteLock();
        }
        
        Core.Logger.Debug("Update edge from node "+producerNodeId+" to node "+consumerNodeId+" success");
    }
        
    //检查更新边合法性
    private static void CheckUpdateValid(RuntimeNode producerNode,RuntimeNode consumerNode,string producerNodePointName, string consumerNodePointName,out Value producerNodePointValue,out Value consumerNodePointValue,out bool hasConsumerNodeLinkedOtherNode)
    {
        //读锁
        GraphLock.EnterReadLock();
        try
        {
            //检查出点是否存在于生产者节点中
            if (!producerNode.Points.Output.TryGetValue(producerNodePointName, out var edges))
            {
                throw new UbikException("Output point " + producerNodePointName + " not exist ");
            }
            //检查生产者节点是否已经连接了当前消费者节点
            if (edges.Any(e => e.NodeId == consumerNode.Id))
            {
                throw new UbikException("Output point already linked in node " + producerNode.Node.Name);
            }
            producerNodePointValue = producerNode.Node.Output.Find(e => e.Name == producerNodePointName);
            
            //检查入点是否存在于消费者节点中
            if (!consumerNode.Points.Input.TryGetValue(consumerNodePointName, out var edge))
            {
                throw new UbikException("Input point "+consumerNodePointName+" not exist in node " + consumerNode.Node.Name);
            }
            consumerNodePointValue = edge.Value;
                
            //检查入点是否已经和其他(包括生产者节点)节点连接
            hasConsumerNodeLinkedOtherNode = edge.NodeId != 0;
            
            //检查属性是否一致
            if (producerNode.Node.Output.First(e => e.Name == producerNodePointName).Attribute != consumerNode.Node.Input.First(e => e.Name == consumerNodePointName).Attribute)
            {
                throw new UbikException("Output point "+producerNodePointName+" and input point "+consumerNodePointName+" attribute not match");
            }
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
    public static void DeleteEdge(int producerNodeId,int consumerNodeId,string producerNodePointName,string consumerNodePointName)
    {
        Core.Logger.Debug("Delete edge from node "+producerNodeId+" to node "+consumerNodeId);
        
        CheckDeleteEdge(producerNodeId,consumerNodeId, producerNodePointName,consumerNodePointName, out var producerNode, out var consumerNode);
        
        //写锁
        GraphLock.EnterWriteLock();
        try
        {
            //删除生产者节点中的边
            producerNode.Points.Output[producerNodePointName].RemoveAll(e => e.NodeId == consumerNodeId);
            
            //重置消费者节点中的边
            var v=consumerNode.Points.Input[consumerNodePointName].Value;
            consumerNode.Points.Input[consumerNodePointName] = new Edge(0, v);
        }
        finally
        {
            GraphLock.ExitWriteLock();
        }
        
        Core.Logger.Debug("Delete edge from node "+producerNodeId+" to node "+consumerNodeId+" success");
    }

    private static void CheckDeleteEdge(int producerNodeId,int consumerNodeId,string producerNodePointName,string consumerNodePointName,out RuntimeNode producerNode,out RuntimeNode consumerNode)
    {
        //读锁
        GraphLock.EnterReadLock();
        try
        {
            //检查生产者节点是否存在
            if (!_runtimeNodes.TryGetValue(producerNodeId, out var producerNodeTmp))
            {
                throw new UbikException("Node "+producerNodeId+" not exist ");
            }
            //检查出点是否存在于生产者节点中
            if (!producerNodeTmp.Points.Output.TryGetValue(producerNodePointName, out var edges))
            {
                throw new UbikException("Output point " + producerNodePointName + " not exist ");
            }
            //检查生产者节点是否已经连接了当前消费者节点
            if (edges.All(e => e.NodeId != consumerNodeId))
            {
                throw new UbikException("Output point not linked in node " + producerNodeTmp.Node.Name);
            }
                
            //检查消费者节点是否存在
            if (!_runtimeNodes.TryGetValue(consumerNodeId, out var consumerNodeTmp))
            {
                throw new UbikException("Node "+consumerNodeId+" not exist ");
            }
            //检查入点是否存在于消费者节点中
            if (!consumerNodeTmp.Points.Input.TryGetValue(consumerNodePointName, out var edge))
            {
                throw new UbikException("Input point " + consumerNodePointName + " not exist in node " +
                                                 consumerNodeTmp.Node.Name);
            }
            //检查入点是否已经生产者节点连接
            if (edge.NodeId != producerNodeId)
            {
                throw new UbikException("Input point not linked in node " + consumerNodeTmp.Node.Name);
            }
                
            producerNode = producerNodeTmp;
            consumerNode = consumerNodeTmp;
        }
        finally
        {
            GraphLock.ExitReadLock();
        }
    }
    
    private static RuntimeNode GetRuntimeNode(int nodeId)
    {
        GraphLock.EnterReadLock();
        try
        {
            if (!_runtimeNodes.TryGetValue(nodeId, out var node))
            {
                throw new UbikException("Node "+nodeId+" not exist ");
            }
            return node;
        }
        finally
        {
            GraphLock.ExitReadLock();
        }
    }
    
    //测试接口//
    //仅供测试使用，生产环境请勿调用//
    public static async void SendMessage(int nodeId,string attribute,object message)
    {
        Core.Logger.Debug("Send message from node "+nodeId+" with attribute "+attribute);
        
        //获取runtime node
        if (!_runtimeNodes.TryGetValue(nodeId, out var node))
        {
            throw new UbikException("Node "+nodeId+" not exist ");
        }
        //发送消息
        node.SendMessage(attribute,message);
        
        Core.Logger.Debug("Send message from node "+nodeId+" with attribute "+attribute+" success");
    }
    
    
    //测试接口//
    //仅供测试使用，生产环境请勿调用//
    public static async Task<object> ReceiveMessage(int nodeId,string attribute)
    {
        Core.Logger.Debug("Receive message from node "+nodeId+" with attribute "+attribute);
        
        //获取runtime node
        if (!_runtimeNodes.TryGetValue(nodeId, out var node))
        {
            throw new UbikException("Node "+nodeId+" not exist ");
        }
        //接收消息
        var result = await node.ReceiveMessage(attribute);
        
        Core.Logger.Debug("Receive message from node "+nodeId+" with attribute "+attribute+" success");
        
        return result;
    }
}