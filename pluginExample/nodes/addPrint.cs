// using ubique.util;

using System.Threading.Channels;
using ubique.plugin;

namespace PluginExample.nodes;

public class AddPrint:IRuntimeNode
{
    private Communicator _communicator;
    // private UbikLogger _logger;

    private Channel<int> _numAChannel = Channel.CreateUnbounded<int>();
    private Channel<int> _numBChannel = Channel.CreateUnbounded<int>();
    
    public bool GetUserParams(string key, object value)
    {
        return true;
    }
    
    public bool GetCommunicator(Communicator communicator)
    {
        _communicator = communicator;
        return true;
    }
    
    public bool Stop()
    {
        return true;
    }

    public bool Run()
    {
        GetNumA();
        GetNumB();
        SendOutput();
        return true;
    }

    private void GetNumA()
    {
        Task.Run(() =>
        {
            while (true)
            {
                var newNum = _communicator.Receive("num_A");
                var numA = (int)newNum.Result;
                _numAChannel.Writer.WriteAsync(numA);
            }
        });
    }

    private void GetNumB()
    {
        Task.Run(() =>
        {
            while (true)
            {
                var newNum = _communicator.Receive("num_B");
                var numB = (int)newNum.Result;
                _numBChannel.Writer.WriteAsync(numB);
            }
        });
    }
    
    private void SendOutput()
    {
        Task.Run(() =>
        {
            while (true)
            {
                var numA = _numAChannel.Reader.ReadAsync();
                var numB = _numBChannel.Reader.ReadAsync();

                var result = numA.Result + numB.Result;
                
                Console.WriteLine(numA+"+"+numB+"="+result);
                
                Thread.Sleep(1000);
        
                _communicator.Send("out",result);
            }
        });
    }
}