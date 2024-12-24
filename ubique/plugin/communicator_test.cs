using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ubique.plugin;

[TestFixture]
public class CommunicatorDLTest
{
    [Test, AsyncStateMachine(typeof(CommunicatorDLTest))]
    public async Task TestCommunicatorDL()
    {
        Console.WriteLine("Start TestCommunicatorDL Send and Receive");
        var communicatorA = new CommunicatorDL(1);
        var communicatorB = new CommunicatorDL(2);
        
        var consumerBuffersA = new Dictionary<string, List<ConsumerBuffer>>();
        var consumerBufferA = new Dictionary<string, ConsumerBuffer>();
        
        var consumerBuffersB = new Dictionary<string, List<ConsumerBuffer>>();
        var consumerBufferB = new Dictionary<string, ConsumerBuffer>();
        
        consumerBufferB.Add("test", new ConsumerBuffer());
        consumerBuffersA.Add("test", new List<ConsumerBuffer>());
        consumerBuffersA["test"].Add(consumerBufferB["test"]);
        
        communicatorA.SetBuffer(consumerBuffersA, consumerBufferA);
        communicatorB.SetBuffer(consumerBuffersB, consumerBufferB);
        
        var testThreadA = new TestThread(communicatorA);
        var testThreadB = new TestThread(communicatorB);
        
        var receiveTask = Task.Run(() => testThreadB.Receive());
        
        Thread.Sleep(1000);


        for (var i = 0; i < 10; i++)
        {
            testThreadA.Send("message " + i);
        }
        
        Thread.Sleep(1000);
        Console.WriteLine("TestCommunicatorDL completed");
    }

    [Test, AsyncStateMachine(typeof(CommunicatorDLTest))]
    public async Task SendMessageToMultipleConsumers()
    {
        var communicatorA = new CommunicatorDL(1);
        var communicatorB = new CommunicatorDL(2);
        var communicatorC = new CommunicatorDL(3);
    
        var consumerBufferA = new ConsumerBuffer();
        var consumerBufferB = new ConsumerBuffer();
        var consumerBufferC = new ConsumerBuffer();
    
        var consumerBuffersA = new Dictionary<string, List<ConsumerBuffer>>
        {
            { "point1", new List<ConsumerBuffer> { consumerBufferB,consumerBufferC } }
        };
        var consumerBuffersB = new Dictionary<string, List<ConsumerBuffer>>{ };
        var consumerBuffersC = new Dictionary<string, List<ConsumerBuffer>>{ };

        communicatorA.SetBuffer(consumerBuffersA, new Dictionary<string, ConsumerBuffer> { { "test", consumerBufferA } });
        communicatorB.SetBuffer(consumerBuffersB, new Dictionary<string, ConsumerBuffer> { { "point1", consumerBufferB } });
        communicatorC.SetBuffer(consumerBuffersC, new Dictionary<string, ConsumerBuffer> { { "point1", consumerBufferC } });
    

        var sendTask = Task.Run(() => communicatorA.Send("point1", "test message"));
        var receiveTaskA = communicatorB.Receive("point1");
        var receiveTaskB = communicatorC.Receive("point1");

        await sendTask;
        var result1 = await receiveTaskA;
        var result2 = await receiveTaskB;

        Assert.That(result1, Is.EqualTo("test message"));
        Assert.That(result2, Is.EqualTo("test message"));
        Console.WriteLine(result1);
        Console.WriteLine(result2);
    }

    [Test, AsyncStateMachine(typeof(CommunicatorDLTest))]
    public async Task MultipleProducerSendMessageToConsumer()
    {
        var communicatorA = new CommunicatorDL(1);
        var communicatorB = new CommunicatorDL(2);
        var communicatorC = new CommunicatorDL(3);
    
        var consumerBufferA = new ConsumerBuffer();
        var consumerBufferB = new ConsumerBuffer();
        var consumerBufferC = new ConsumerBuffer();
    
        var consumerBuffersA = new Dictionary<string, List<ConsumerBuffer>>
        {
            { "point1", new List<ConsumerBuffer> { consumerBufferC } }
        };
        var consumerBuffersB = new Dictionary<string, List<ConsumerBuffer>>
        {
            { "point1", new List<ConsumerBuffer> { consumerBufferC } }
        };
        var consumerBuffersC = new Dictionary<string, List<ConsumerBuffer>>{ };
    
        communicatorA.SetBuffer(consumerBuffersA, new Dictionary<string, ConsumerBuffer> { { "point1", consumerBufferA } });
        communicatorB.SetBuffer(consumerBuffersB, new Dictionary<string, ConsumerBuffer> { { "point1", consumerBufferB } });
        communicatorC.SetBuffer(consumerBuffersC, new Dictionary<string, ConsumerBuffer> { { "point1", consumerBufferC } });
    
        var sendTaskA = Task.Run(() => communicatorA.Send("point1", "test message from A"));
        var sendTaskB = Task.Run(() => communicatorB.Send("point1", "test message from B"));
    
        await sendTaskA;
        await sendTaskB;
    
        var result1 = await communicatorC.Receive("point1");
        var result2 = await communicatorC.Receive("point1");
    
        Console.WriteLine(result1);
        Console.WriteLine(result2);
    }

    [Test, AsyncStateMachine(typeof(CommunicatorDLTest))]
    public async Task MultipleProducerSendMessageToMultipleConsumers()
    {
        var communicatorA = new CommunicatorDL(1);
        var communicatorB = new CommunicatorDL(2);
        var communicatorC = new CommunicatorDL(3);
    
        var consumerBufferA = new ConsumerBuffer();
        var consumerBufferB = new ConsumerBuffer();
        var consumerBufferC = new ConsumerBuffer();
    
        var consumerBuffersA = new Dictionary<string, List<ConsumerBuffer>>
        {
            { "point1", new List<ConsumerBuffer> { consumerBufferB,consumerBufferC } }
        };
        var consumerBuffersB = new Dictionary<string, List<ConsumerBuffer>>
        {
            { "point1", new List<ConsumerBuffer> { consumerBufferC } }
        };
        var consumerBuffersC = new Dictionary<string, List<ConsumerBuffer>>{ };
    
        communicatorA.SetBuffer(consumerBuffersA, new Dictionary<string, ConsumerBuffer> { { "point1", consumerBufferA } });
        communicatorB.SetBuffer(consumerBuffersB, new Dictionary<string, ConsumerBuffer> { { "point1", consumerBufferB } });
        communicatorC.SetBuffer(consumerBuffersC, new Dictionary<string, ConsumerBuffer> { { "point1", consumerBufferC } });
    
        var sendTaskA = Task.Run(() => communicatorA.Send("point1", "test message from A"));
        var sendTaskB = Task.Run(() => communicatorB.Send("point1", "test message from B"));
    
        await sendTaskA;
        await sendTaskB;
    
        var result0=await communicatorB.Receive("point1");
        var result1 = await communicatorC.Receive("point1");
        var result2 = await communicatorC.Receive("point1");
    
        Console.WriteLine(result0);
        Console.WriteLine(result1);
        Console.WriteLine(result2);
    }

    [Test, AsyncStateMachine(typeof(CommunicatorDLTest))]
    public async Task SendMessageToMultipleConsumersFromDifferentPoints()//Even more, B send message to it self.
    {
        var communicatorA = new CommunicatorDL(1);
        var communicatorB = new CommunicatorDL(2);
        var communicatorC = new CommunicatorDL(3);
    
        var consumerBufferA = new ConsumerBuffer();
        var consumerBufferB = new ConsumerBuffer();
        var consumerBufferC = new ConsumerBuffer();
    
        var consumerBuffersA = new Dictionary<string, List<ConsumerBuffer>>
        {
            { "point1", new List<ConsumerBuffer> { consumerBufferB } },
            { "point2", new List<ConsumerBuffer> { consumerBufferC } }
        };
        var consumerBuffersB = new Dictionary<string, List<ConsumerBuffer>>
        {
            { "point1", new List<ConsumerBuffer> { consumerBufferB } }
        };
        var consumerBuffersC = new Dictionary<string, List<ConsumerBuffer>>
        {
            { "point2", new List<ConsumerBuffer> { consumerBufferC } }
        };
    
        communicatorA.SetBuffer(consumerBuffersA, new Dictionary<string, ConsumerBuffer> { { "point1", consumerBufferA } });
        communicatorB.SetBuffer(consumerBuffersB, new Dictionary<string, ConsumerBuffer> { { "point1", consumerBufferB } });
        communicatorC.SetBuffer(consumerBuffersC, new Dictionary<string, ConsumerBuffer> { { "point2", consumerBufferC } });
    
        var sendTaskA = Task.Run(() => communicatorA.Send("point1", "test message from A"));
        var sendTaskB = Task.Run(() => communicatorA.Send("point2", "test message from A"));
        var sendTaskC = Task.Run(() => communicatorB.Send("point1", "test message from B"));
    
        await sendTaskA;
        await sendTaskB;
    
        var result1 = await communicatorB.Receive("point1");
        var result2 = await communicatorC.Receive("point2");
        var result3 = await communicatorB.Receive("point1");
    
        Console.WriteLine(result1);
        Console.WriteLine(result2);
        Console.WriteLine(result3);
    }

    private class TestThread(CommunicatorDL communicator)
    {
        public void Send(string message)
        {
            Console.WriteLine("Sending message: " + message);
            communicator.Send("test", message);
        }

        public async Task Receive()
        {
            Console.WriteLine("Start receiving messages");
            while(true){
                var message = communicator.Receive("test");
                Console.WriteLine("Received message: " + message.Result);
            }
        }
    }
}