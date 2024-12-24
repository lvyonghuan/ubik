using System.Threading.Channels;
using ubique.util;

namespace ubique.plugin;

//The communicator allocates a buffer created by a circular
//queue and a semaphore to a pair of points.
public interface Communicator
{
    public void SetBuffer(Dictionary<string,List<ConsumerBuffer>> consumerBuffers,Dictionary<string,ConsumerBuffer> consumerBuffer);
}

public class ConsumerBuffer
{
    public Channel<object> Ch = Channel.CreateUnbounded<object>();
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

    public async void Send(string pointName,object message)
    {
        _logger.Debug("Sending data in point: " + pointName + " ,node id: " + _runtimeNodeId);
        var consumerBuffers=_consumerBuffers[pointName];
        foreach (var consumerBuffer in consumerBuffers)
        {
            await consumerBuffer.Ch.Writer.WriteAsync(message);
        }
    }

    public async Task<object> Receive(string pointName)
    {
        var consumerBuffer = _consumerBuffer[pointName];

        _logger.Debug("Waiting for data arrival in point: " + pointName + " ,node id: " + _runtimeNodeId);

        // get the data from the upstream producer.
        var result = await consumerBuffer.Ch.Reader.ReadAsync();
        _logger.Debug("Data arrived in point: " + pointName + " ,node id: " + _runtimeNodeId);

        return result;
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