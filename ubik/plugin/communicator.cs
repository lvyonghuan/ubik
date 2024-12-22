namespace ubik.plugin;

public interface Communicator
{
    public void SetBuffer(List<ConsumerBuffer> consumerBuffers);
}

public class ConsumerBuffer
{
    public object Buffer;
    public Mutex Mutex;
}

public class CommunicatorDL(int runtimeNodeId,object receiverBuffer,object senderBuffer):Communicator
{
    private int _runtimeNodeId=runtimeNodeId;
    private object _receiverBuffer=receiverBuffer;
    private object _senderBuffer=senderBuffer;
    
    private List<ConsumerBuffer> _consumerBuffers=new List<ConsumerBuffer>();
    
    public void SetBuffer(List<ConsumerBuffer> consumerBuffers)
    {
        _consumerBuffers=consumerBuffers;
    }

    public void Send(object message)
    {
        //TODO get the message from the senderBuffer.
        
        foreach (var consumerBuffer in _consumerBuffers)
        {
            //producer will unlock the mutex, and consumer will lock the mutex.
            consumerBuffer.Mutex.ReleaseMutex();
            
            //Then write the message to the buffer.
            consumerBuffer.Buffer=message;
        }
    }
    
    public bool ReportState(int state)
    {
        return true;
    }
}

public class CommunicatorGrpc(int runtimeNodeId):Communicator
{
    private int _runtimeNodeId=runtimeNodeId;
    
    private List<ConsumerBuffer> _consumerBuffers=new List<ConsumerBuffer>();
    
    public void SetBuffer(List<ConsumerBuffer> consumerBuffers)
    {
        _consumerBuffers=consumerBuffers;
    }

    //TODO
}