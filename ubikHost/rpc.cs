using ubique.util;
using Grpc.Core;

namespace ubikHost;

public class Rpc
{
    private static Core _core;

    public Rpc(Core core)
    {
        _core = core;
    }
    
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
                    Services = { Ubique.Host.BindService(new HeartbeatServiceImpl()) },
                    Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
                };
                server.Start();
                Core.Logger.Info("Rpc ping server started on port " + port);
                break; 
            }
            catch (IOException)
            {
                port++;//端口被占用，尝试下一个端口
                if (port > 65535)
                {
                    throw new UbikException("Port number must be between 1 and 65535");
                }
            }
        }
        
        return port;
    }
    
    private class HeartbeatServiceImpl : Ubique.Host.HostBase
    {
        public override Task<Ubique.PongMessage> Handshake(Ubique.HandshakeMessage request, ServerCallContext context)
        {
            //获取plugin名称,端口
            var pluginName = request.Name;
            var pluginPort = request.Port;
            
            //向插件设置端口
            try
            {
                _core.SetPluginPort(pluginName,pluginPort);
            }
            catch (UbikException e)
            {
                Core.Logger.Error(e);
                return Task.FromResult(new Ubique.PongMessage
                {
                    State = 500,//TODO：换成枚举
                });
            }
            
            return Task.FromResult(new Ubique.PongMessage
            {
                State = 200,//TODO：换成枚举
            });
        }
        
        public override Task<Ubique.PongMessage> Ping(Ubique.PingMessage request, ServerCallContext context)
        {
            return Task.FromResult(new Ubique.PongMessage
            {
                State = 200,//TODO：换成枚举
            });
        }
    }
}
