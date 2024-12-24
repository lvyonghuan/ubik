using NUnit.Framework;

namespace ubikHost;

public class TestGraph
{
    [Test]
    public void TestAddRuntimeNode()
    {
        var node = new Node("test", "test", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        node.AddRuntimeNode();
        
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        nodeA.AddRuntimeNode();
    }

    [Test]
    public void TestDeleteNode()
    {
        var node = new Node("test", "test", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        var id=node.AddRuntimeNode();
        
        Graph.RemoveNode(id);
    }

    [Test]
    public void TestAddEdge()
    {
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeB = new Node("testB", "testB", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        
        var valueA = new Value("test","test","string","test");
        var valueB = new Value("test","test","string","test");
        
        nodeA.Output.Add(valueA);
        nodeB.Input.Add(valueB);
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idB, valueA);
    }
    
    [Test]
    public void TestDeleteEdge()
    {
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeB = new Node("testB", "testB", true,false, new List<Value>(), new List<Value>(), new List<Value>());
     
        var valueA = new Value("test","test","string","test");
        var valueB = new Value("test","test","string","test");
        
        nodeA.Output.Add(valueA);
        nodeB.Input.Add(valueB);
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idB, valueA);
        
        Graph.DeleteEdge(idA, idB,valueA.Attribute);
    }

    [Test]
    public void TestAddMoreEdges()
    {
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeB = new Node("testB", "testB", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeC = new Node("testB", "testB", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        
        var valueA = new Value("test","test","string","test");
        var valueB = new Value("test","test","string","test");
        var valueC = new Value("test","test","string","test");
        
        nodeA.Output.Add(valueA);
        nodeB.Input.Add(valueB);
        nodeC.Input.Add(valueC);
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        var idC=nodeC.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idB, valueA);
        Graph.UpdateEdge(idA, idC, valueA);
    }

    [Test]
    public void TestUpdateEdge()
    {
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeB = new Node("testB", "testB", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        var nodeC = new Node("testB", "testB", true,false, new List<Value>(), new List<Value>(), new List<Value>());
        
        var valueA = new Value("test","test","string","test");
        var valueB = new Value("test","test","string","test");
        var valueC = new Value("test","test","string","test");
        
        nodeA.Output.Add(valueA);
        nodeB.Output.Add(valueB);
        nodeC.Input.Add(valueC);
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        var idC=nodeC.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idC, valueA);
        Graph.UpdateEdge(idB, idC, valueB);
    }
}