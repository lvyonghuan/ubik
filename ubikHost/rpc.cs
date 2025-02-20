using ubique.util;
using Grpc.Core;

namespace ubikHost;

public class Rpc
{
    public void Init(int port=5001)
    {
        if (port <= 0 || port > 65535)
        {
            throw new UbikException("Port number must be between 1 and 65535");
        }
        
        // var server = new Server
        // {
        //     Services = { ubique.BindService(new GreeterImpl()) },
        //     Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
        // };
    }
}