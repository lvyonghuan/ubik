namespace ubikCore;

public class Graph{
    private Dictionary<int,RuntimeNode> _runtimeNodes = new Dictionary<int, RuntimeNode>(); //运行时节点, key为节点ID，value为节点
    private List<RuntimeNode> _enterNodes = new List<RuntimeNode>(); // 入口节点
    private static ReaderWriterLockSlim _graphLock = new ReaderWriterLockSlim(); // 图读写锁    
    
    //运行时节点
    //被转载的节点
    public class RuntimeNode
    {
        //运行时节点ID
        public int Id { get; private set; }
        public int State = Ready; //节点状态

        private static int _nextId = 0;
        private Node _node;
        private bool _hasVisited = false; //是否已经被访问过，用于图的合法性检查

        //节点状态定义
        public const int Running = 0;
        public const int Ready = 1;
        public const int Failed = 2;

        public RuntimeNode(Node node)
        {
            Id = _nextId++;
            _node = node;
        }

        //运行时节点出入点
        // 连接节点
        //output point<->input point
        public class RuntimeNodePoints
        {
            private Dictionary<string, List<Edge>>
                _output = new Dictionary<string, List<Edge>>(); // 输出点，key为参数名称，value ID值为输入点所在节点ID的集合

            private Dictionary<string, Edge> _input = new Dictionary<string, Edge>(); // 输入点，key为参数名称，value ID值为输出点所在节点ID

            //添加出边
            public void AddOutput()
            {

            }

            //检查连接入点合法性
            private void CheckLinkInputValid(RuntimeNode linkNode, string attribute)
            {
                //读锁
                _graphLock.EnterReadLock();
                try
                {
                    //检查入点是否存在,入点是否已经和其他节点连接
                    
                }
                finally
                {
                    _graphLock.ExitReadLock();
                }
            }
        }
    }
}