using ubique.plugin;

namespace PluginExample.nodes;

public class Increasing:IRuntimeNode
{
    private Communicator _communicator;
    
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
        GetInput();
        return true;
    }

    private void GetInput()
    {
        Task.Run(() =>
        {
            while (true)
            {
                var newNum = _communicator.Receive("receive");
                var num = (int)newNum.Result;
                SendOutput(++num);
            }
        });
    }
    
    private void SendOutput(int num)
    {
        _communicator.Send("out",num);
    }
}