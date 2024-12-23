using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ubik.plugin;

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
                Console.WriteLine("Received message: " + message);
            }
        }
    }
}