using ubique.util;

namespace ubique.plugin;

public interface Communicator
{
    public void SetBuffer(Dictionary<string,List<ConsumerBuffer>> consumerBuffers,Dictionary<string,ConsumerBuffer> senderConsumerBuffer);
}

public class ConsumerBuffer
{
    public Queue<object> Buffer=new Queue<object>();
    public readonly Semaphore Semaphore=new Semaphore(0,int.MaxValue);
}

public class CommunicatorDL(int runtimeNodeId):Communicator
{
    private UbikLogger _logger=new UbikLogger(UbikLogger.DebugLevel,false,"");
        
    private int _runtimeNodeId=runtimeNodeId;
    
    private Dictionary<string,List<ConsumerBuffer>> _consumerBuffers=new Dictionary<string,List<ConsumerBuffer>>();
    private Dictionary<string,ConsumerBuffer> _consumerBuffer=new Dictionary<string,ConsumerBuffer>();
    
    public void SetBuffer(Dictionary<string,List<ConsumerBuffer>> consumerBuffers,Dictionary<string,ConsumerBuffer> consumerBuffer)
    {
        _consumerBuffers=consumerBuffers;
        _consumerBuffer=consumerBuffer;
    }

    public void Send(string pointName,object message)
    {
        var consumerBuffers=_consumerBuffers[pointName];
        foreach (var consumerBuffer in consumerBuffers)
        {
            //write the message to the buffer.
            consumerBuffer.Buffer.Enqueue(message);
            
            _logger.Debug("Message sent to point: "+pointName+" ,node id: "+_runtimeNodeId);
            //Then producer will do p operation to notice the consumer.
            consumerBuffer.Semaphore.Release();
        }
    }

    public object Receive(string pointName)
    {
        var consumerBuffer=_consumerBuffer[pointName];
     
        _logger.Debug("Waiting for data arrival in point: "+pointName+" ,node id: "+_runtimeNodeId);
        consumerBuffer.Semaphore.WaitOne();//consumer will do v operation to wait for the data.
        _logger.Debug("Data arrived in point: "+pointName+" ,node id: "+_runtimeNodeId);
        
        //return the data from the upstream producer.
        return consumerBuffer.Buffer.Dequeue();
    }
    
    public bool ReportState(int state)
    {
        return true;
    }
}

public class CommunicatorGrpc(int runtimeNodeId):Communicator
{
    private int _runtimeNodeId=runtimeNodeId;
    
    private Dictionary<string,List<ConsumerBuffer>> _consumerBuffers=new Dictionary<string,List<ConsumerBuffer>>();
    private Dictionary<string,ConsumerBuffer> _consumerBuffer=new Dictionary<string,ConsumerBuffer>();
    
    public void SetBuffer(Dictionary<string,List<ConsumerBuffer>> consumerBuffers,Dictionary<string,ConsumerBuffer> consumerBuffer)
    {
        _consumerBuffers=consumerBuffers;
        _consumerBuffer=consumerBuffer;
    }

    //TODO
}