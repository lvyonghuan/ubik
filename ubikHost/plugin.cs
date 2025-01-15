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

    public void Mount()
    {
        //如果count>0
        //表示该plugin挂载在图上的节点数量大于0
        //需要进行物理挂载
        if (_count>0)
        {
            
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
}