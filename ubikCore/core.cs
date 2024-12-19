namespace ubikCore;

public class Core
{
    private Dictionary<string, Node> _nodes = new Dictionary<string, Node>();
    private Graph _graph = new Graph();
    
    //装载节点
    //节点只能被装载，不能卸载
    public void AddNode(Node node)
    {
        _nodes.Add(node.Name, node);
    }
}