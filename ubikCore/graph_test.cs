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
        
        var nodeA = new Node("testA", "testA", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        nodeA.AddNode();
    }

    [Test]
    public void TestDeleteNode()
    {
        var node = new Node("test", "test", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        var id=node.AddNode();
        
        Graph.RemoveNode(id);
    }

    [Test]
    public void TestAddEdge()
    {
        var nodeA = new Node("testA", "testA", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        var nodeB = new Node("testB", "testB", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        
        nodeA.Output.Add("test","string");
        nodeB.Input.Add("test","string");
        
        var idA=nodeA.AddNode();
        var idB=nodeB.AddNode();
        
        Graph.UpdateEdge(idA, idB, "test");
    }
    
    [Test]
    public void TestDeleteEdge()
    {
        var nodeA = new Node("testA", "testA", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        var nodeB = new Node("testB", "testB", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        
        nodeA.Output.Add("test","string");
        nodeB.Input.Add("test","string");
        
        var idA=nodeA.AddNode();
        var idB=nodeB.AddNode();
        
        Graph.UpdateEdge(idA, idB, "test");
        
        Graph.DeleteEdge(idA, idB,"test");
    }

    [Test]
    public void TestAddMoreEdges()
    {
        var nodeA = new Node("testA", "testA", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        var nodeB = new Node("testB", "testB", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        var nodeC = new Node("testB", "testB", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        
        nodeA.Output.Add("test","string");
        nodeB.Input.Add("test","string");
        nodeC.Input.Add("test","string");
        
        var idA=nodeA.AddNode();
        var idB=nodeB.AddNode();
        var idC=nodeC.AddNode();
        
        Graph.UpdateEdge(idA, idB, "test");
        Graph.UpdateEdge(idA, idC, "test");
    }

    [Test]
    public void TestUpdateEdge()
    {
        var nodeA = new Node("testA", "testA", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        var nodeB = new Node("testB", "testB", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        var nodeC = new Node("testB", "testB", true, new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>());
        
        nodeA.Output.Add("test","string");
        nodeB.Output.Add("test","string");
        nodeC.Input.Add("test","string");
        
        var idA=nodeA.AddNode();
        var idB=nodeB.AddNode();
        var idC=nodeC.AddNode();
        
        Graph.UpdateEdge(idA, idC, "test");
        Graph.UpdateEdge(idB, idC, "test");
    }
}