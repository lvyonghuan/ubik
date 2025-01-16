using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ubikHost;

public class TestGraph
{
    [Test]
    public void TestAddRuntimeNode()
    {
        CoreTest.TestInitCore();
        var plugin = new Plugin();
        
        var node = new Node("test", "test", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        node.AddRuntimeNode();
        
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        nodeA.AddRuntimeNode();
    }

    [Test]
    public void TestDeleteNode()
    {
        CoreTest.TestInitCore();
        var plugin = new Plugin();
        
        var node = new Node("test", "test", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var id=node.AddRuntimeNode();
        
        Graph.RemoveNode(id);
    }

    [Test]
    public void TestAddEdge()
    {
        CoreTest.TestInitCore();
        var plugin = new Plugin();
        
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeB = new Node("testB", "testB", false,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        
        var valueA = new Value("test","test","string","test");
        var valueB = new Value("test","test","string","test");
        
        nodeA.Output.Add(valueA);
        nodeB.Input.Add(valueB);
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idB, valueA.Name, valueB.Name);
    }
    
    [Test]
    public void TestDeleteEdge()
    {
        CoreTest.TestInitCore();
        var plugin = new Plugin();
        
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeB = new Node("testB", "testB", false,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
     
        var valueA = new Value("test","test","string","test");
        var valueB = new Value("test","test","string","test");
        
        nodeA.Output.Add(valueA);
        nodeB.Input.Add(valueB);
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idB, valueA.Name, valueB.Name);
        
        Graph.DeleteEdge(idA, idB,valueA.Name, valueB.Name);
    }

    [Test]
    public void TestAddMoreEdges()
    {
        CoreTest.TestInitCore();
        var plugin = new Plugin();
        
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeB = new Node("testB", "testBB", false,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeC = new Node("testB", "testBC", false,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        
        var valueA = new Value("test","test","string","test");
        var valueB = new Value("test","test","string","test");
        var valueC = new Value("test","test","string","test");
        
        nodeA.Output.Add(valueA);
        nodeB.Input.Add(valueB);
        nodeC.Input.Add(valueC);
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        var idC=nodeC.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idB, valueA.Name, valueB.Name);
        Graph.UpdateEdge(idA, idC, valueA.Name, valueC.Name);
    }

    [Test]
    public void TestUpdateEdge()
    {
        CoreTest.TestInitCore();
        var plugin = new Plugin();
        
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeB = new Node("testB", "testB", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeC = new Node("testB", "testB", false,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        
        var valueA = new Value("test","test","string","test");
        var valueB = new Value("test","test","string","test");
        var valueC = new Value("test","test","string","test");
        
        nodeA.Output.Add(valueA);
        nodeB.Output.Add(valueB);
        nodeC.Input.Add(valueC);
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        var idC=nodeC.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idC, valueA.Name, valueC.Name);
        Graph.UpdateEdge(idB, idC, valueB.Name, valueC.Name);
    }
}