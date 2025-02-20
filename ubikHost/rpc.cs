using ubique.util;
using Grpc.Core;

namespace ubikHost;

public class Rpc
{
    //初始化Ping rpc服务器，返回端口号
    //Ping服务器负责响应Ping请求，对客户端返回Pong
    public int Init(int port=9876)
    {
        if (port <= 0 || port > 65535)
        {
            throw new UbikException("Port number must be between 1 and 65535");
        }

        while (true)
        {
            try
            {
                var server = new Server
                {
                    Services = { Ubique.HeartbeatService.BindService(new HeartbeatServiceImpl()) },
                    Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
                };
                server.Start();
                // Core.Logger.Info("Rpc ping server started on port " + port);
                break; 
            }
            catch (IOException)
            {
                port++;//端口被占用，尝试下一个端口
            }
        }
        
        return port;
    }
    
    private class HeartbeatServiceImpl : Ubique.HeartbeatService.HeartbeatServiceBase
    {
        public override Task<Ubique.PongMessage> Ping(Ubique.PingMessage request, ServerCallContext context)
        {
            return Task.FromResult(new Ubique.PongMessage
            {
                Message = "Pong"
            });
        }
    }
}

