using NUnit.Framework;

namespace ubikHost;

public class TestGraph
{
    [Test]
    public void TestAddRuntimeNode()
    {
        var node = new Node("test", "test", true, new List<Value>(), new List<Value>(), new List<Value>());
        node.AddRuntimeNode();
        
        var nodeA = new Node("testA", "testA", true, new List<Value>(), new List<Value>(), new List<Value>());
        nodeA.AddRuntimeNode();
    }

    [Test]
    public void TestDeleteNode()
    {
        var node = new Node("test", "test", true, new List<Value>(), new List<Value>(), new List<Value>());
        var id=node.AddRuntimeNode();
        
        Graph.RemoveNode(id);
    }

    [Test]
    public void TestAddEdge()
    {
        var nodeA = new Node("testA", "testA", true, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeB = new Node("testB", "testB", true, new List<Value>(), new List<Value>(), new List<Value>());
        
        nodeA.Output.Add(new Value("test","test","string","test"));
        nodeB.Input.Add(new Value("test","test","string","test"));
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idB, "test");
    }
    
    [Test]
    public void TestDeleteEdge()
    {
        var nodeA = new Node("testA", "testA", true, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeB = new Node("testB", "testB", true, new List<Value>(), new List<Value>(), new List<Value>());
        
        nodeA.Output.Add(new Value("test","test","string","test"));
        nodeB.Input.Add(new Value("test","test","string","test"));
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idB, "test");
        
        Graph.DeleteEdge(idA, idB,"test");
    }

    [Test]
    public void TestAddMoreEdges()
    {
        var nodeA = new Node("testA", "testA", true, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeB = new Node("testB", "testB", true, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeC = new Node("testB", "testB", true, new List<Value>(), new List<Value>(), new List<Value>());
        
        nodeA.Output.Add(new Value("test","test","string","test"));
        nodeB.Input.Add(new Value("test","test","string","test"));
        nodeC.Input.Add(new Value("test","test","string","test"));
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        var idC=nodeC.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idB, "test");
        Graph.UpdateEdge(idA, idC, "test");
    }

    [Test]
    public void TestUpdateEdge()
    {
        var nodeA = new Node("testA", "testA", true, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeB = new Node("testB", "testB", true, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeC = new Node("testB", "testB", true, new List<Value>(), new List<Value>(), new List<Value>());
        
        nodeA.Output.Add(new Value("test","test","string","test"));
        nodeB.Output.Add(new Value("test","test","string","test"));
        nodeC.Input.Add(new Value("test","test","string","test"));
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        var idC=nodeC.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idC, "test");
        Graph.UpdateEdge(idB, idC, "test");
    }
}