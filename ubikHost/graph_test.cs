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
        
        Graph.UpdateEdge(idA, idB, valueA, valueB);
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
        
        Graph.UpdateEdge(idA, idB, valueA, valueB);
        
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
        
        Graph.UpdateEdge(idA, idB, valueA, valueB);
        Graph.UpdateEdge(idA, idC, valueA, valueC);
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
        
        Graph.UpdateEdge(idA, idC, valueA, valueC);
        Graph.UpdateEdge(idB, idC, valueB, valueC);
    }
    
    [Test, AsyncStateMachine(typeof(Graph.RuntimeNode))]
    public async Task TestSendData()
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
        
        Graph.UpdateEdge(idA, idB, valueA, valueB);
        Graph.BeforeRunSet();
        
        var sendTaskA=Task.Run(()=>Graph.SendMessage(idA,valueA.Name, "test message from A"));
        var receiveTaskB=Task.Run(()=>Graph.ReceiveMessage(idB,valueB.Name));
        
        await sendTaskA;
        var result=await receiveTaskB;
        Console.WriteLine(result);
    }

    [Test, AsyncStateMachine(typeof(Graph.RuntimeNode))]
    public async Task TestSendDataWithDifferentPoints()
    {
        CoreTest.TestInitCore();
        var plugin = new Plugin();
        
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeB = new Node("testB", "testB", false,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeC = new Node("testC", "testC", false,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        
        var valueA1 = new Value("test1","test1","string","test");
        var valueA2 = new Value("test2","test2","string","test");
        var valueB = new Value("test1","test","string","test");
        var valueC=new Value("test2","test","string","test");
        
        nodeA.Output.Add(valueA1);
        nodeA.Output.Add(valueA2);
        nodeB.Input.Add(valueB);
        nodeC.Input.Add(valueC);
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        var idC = nodeC.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idB, valueA1, valueB);
        Graph.UpdateEdge(idA, idC, valueA2, valueC);
        
        Graph.BeforeRunSet();
        
        var sendTaskA1=Task.Run(()=>Graph.SendMessage(idA,valueA1.Name, "test message from A1"));
        var sendTaskA2=Task.Run(()=>Graph.SendMessage(idA,valueA2.Name, "test message from A2"));
        var receiveTaskB=Task.Run(()=>Graph.ReceiveMessage(idB,valueB.Name));
        var receiveTaskC=Task.Run(()=>Graph.ReceiveMessage(idC,valueC.Name));
        
        await sendTaskA1;
        await sendTaskA2;
        var resultB=await receiveTaskB;
        var resultC=await receiveTaskC;
        
        Console.WriteLine(resultB);
        Console.WriteLine(resultC);
    }

    [Test, AsyncStateMachine(typeof(Graph.RuntimeNode))]
    public async Task TestSendMessageChain()
    {
        CoreTest.TestInitCore();
        var plugin = new Plugin();
        
        var nodeA = new Node("testA", "testA", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeB = new Node("testB", "testB", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeC = new Node("testC", "testC", true,false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        
        var valueA = new Value("test1","testA","string","test");
        var valueB1 = new Value("test1","testB1","string","test");
        var valueB2=new Value("test2","testB2","string","test");
        var valueC=new Value("test2","testC","string","test");
        
        nodeA.Output.Add(valueA);
        nodeB.Input.Add(valueB1);
        nodeB.Output.Add(valueB2);
        nodeC.Input.Add(valueC);
        
        var idA=nodeA.AddRuntimeNode();
        var idB=nodeB.AddRuntimeNode();
        var idC = nodeC.AddRuntimeNode();
        
        Graph.UpdateEdge(idA, idB, valueA, valueB1);
        Graph.UpdateEdge(idB, idC, valueB2, valueC);
        
        Graph.BeforeRunSet();
        
        var sendTaskA=Task.Run(()=>Graph.SendMessage(idA,valueA.Name, "test message from A"));
        var receiveTaskB1=Task.Run(()=>Graph.ReceiveMessage(idB,valueB1.Name));
        var receiveTaskC=Task.Run(()=>Graph.ReceiveMessage(idC,valueC.Name));
        
        await sendTaskA;
        var resultB1=await receiveTaskB1;
        var sendTaskB2=Task.Run(()=>Graph.SendMessage(idB,valueB2.Name, receiveTaskB1.Result+", test message from B2"));
        await sendTaskB2;
        var resultC=await receiveTaskC;
        
        Console.WriteLine(resultB1);
        Console.WriteLine(resultC);
    }

    [Test, AsyncStateMachine(typeof(Graph.RuntimeNode))]
    public async Task TestSendMessageChainWithDifferentPoints()
    {
        CoreTest.TestInitCore();
        var plugin = new Plugin();
        
        var nodeA = new Node("testA", "testA", true, false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeB = new Node("testB", "testB", true, false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);
        var nodeC = new Node("testC", "testC", true, false, new List<Value>(), new List<Value>(), new List<Value>(), plugin);

        var valueA1 = new Value("test1", "testA1", "string", "test");
        var valueB1 = new Value("test1", "testB1", "string", "test");
        var valueB2 = new Value("test2", "testB2", "string", "test");
        var valueC1 = new Value("test1", "testC1", "string", "test");
        var valueC2 = new Value("test2", "testC2", "string", "test");

        nodeA.Output.Add(valueA1);
        nodeB.Input.Add(valueB1);
        nodeB.Output.Add(valueB2);
        nodeC.Input.Add(valueC1);
        nodeC.Input.Add(valueC2);

        var idA = nodeA.AddRuntimeNode();
        var idB = nodeB.AddRuntimeNode();
        var idC = nodeC.AddRuntimeNode();

        Graph.UpdateEdge(idA, idB, valueA1, valueB1);
        Graph.UpdateEdge(idA, idC, valueA1, valueC1);
        Graph.UpdateEdge(idB, idC, valueB2, valueC2);

        Graph.BeforeRunSet();

        var sendTaskA1 = Task.Run(() => Graph.SendMessage(idA, valueA1.Name, "test message from A1"));
        var receiveTaskB1 = Task.Run(() => Graph.ReceiveMessage(idB, valueB1.Name));
        var receiveTaskC1 = Task.Run(() => Graph.ReceiveMessage(idC, valueC1.Name));
        var receiveTaskC2 = Task.Run(() => Graph.ReceiveMessage(idC, valueC2.Name));

        await sendTaskA1;

        var resultB1 = await receiveTaskB1;
        var sendTaskB2 = Task.Run(() => Graph.SendMessage(idB, valueB2.Name, resultB1 + ", test message from B2"));
        await sendTaskB2;
        var resultC1 = await receiveTaskC1;
        var resultC2 = await receiveTaskC2;

        Console.WriteLine(resultB1);
        Console.WriteLine(resultC1);
        Console.WriteLine(resultC2);
    }
    
    [Test, AsyncStateMachine(typeof(Graph.RuntimeNode))]
    public async Task TestReceiveBeforeSend()
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
        
        Graph.UpdateEdge(idA, idB, valueA, valueB);
        Graph.BeforeRunSet();
        
        var receiveTaskB=Task.Run(()=>Graph.ReceiveMessage(idB,valueB.Name));
        var sendTaskA=Task.Run(()=>Graph.SendMessage(idA,valueA.Name, "test message from A"));
        
        var result=await receiveTaskB;
        await sendTaskA;
        
        Console.WriteLine(result);
    }
}