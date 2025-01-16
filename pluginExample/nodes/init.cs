using ubique.plugin;

namespace PluginExample.nodes;

public class Init:IRuntimeNode
{
    private Communicator _communicator;

    private int initNum = 1;
    
    public bool GetUserParams(string key, object value)
    {
        switch (key)
        {
            case "init_num":
                initNum = (int)value;
                break;
            default:
                return false;
        }
        
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
        SendOutput();
        return true;
    }

    private void GetInput()
    {
        Task.Run(() =>
        {
            while (true)
            {
                var newNum = _communicator.Receive("receive");
                initNum = (int)newNum.Result;
                SendOutput();
            }
        });
    }
    
    private void SendOutput()
    {
        _communicator.Send("out",initNum);
    }
}