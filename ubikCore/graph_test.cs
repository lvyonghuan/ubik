using System.Collections.Generic;

namespace ubikCore;
using NUnit.Framework;

public class TestGraph
{
    [Test]
    public void TestAddNode()
    {
        var node = new Node("test", "test", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        node.AddNode();
    }

    [Test]
    public void TestDeleteNode()
    {
        var node = new Node("test", "test", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        var id=node.AddNode();
        
        Graph.RemoveNode(id);
    }
}