// using ubique.util;

using System.Threading.Channels;
using ubique.plugin;

namespace PluginExample.nodes;

public class AddPrint:IRuntimeNode
{
    private Communicator _communicator;
    // private UbikLogger _logger;

    private Channel<int> _numAChannel = Channel.CreateBounded<int>(1);
    private Channel<int> _numBChannel = Channel.CreateBounded<int>(1);
    
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
        Task.Run(async () =>
        {
            while (true)
            {
                var newNum = _communicator.Receive("num_A");
                var numA = (int)newNum.Result;
                await _numAChannel.Writer.WriteAsync(numA);
            }
        });
    }

    private void GetNumB()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                var newNum = _communicator.Receive("num_B");
                var numB = (int)newNum.Result;
                await _numBChannel.Writer.WriteAsync(numB);
            }
        });
    }
    
    private void SendOutput()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                var numA = await _numAChannel.Reader.ReadAsync();
                var numB = await _numBChannel.Reader.ReadAsync();

                var result = numA + numB;
                
                Console.WriteLine(numA+"+"+numB+"="+result);
                
                Thread.Sleep(1000);
        
                _communicator.Send("out",result);
            }
        });
    }
}