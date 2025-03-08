using ubique.util;
using Grpc.Core;
using Ubique;

namespace ubique.plugin;

public class GrpcClient
{
    private static IClientHandel _clientHandel;
    
    //Start a GRPC server on pluginPort
    //Handshake with host
    //Start ping to host
    public GrpcClient(int pluginPort, int hostPort,string pluginName,IClientHandel clientHandel)
    {
        _clientHandel = clientHandel;
        
        //Start a gRPC server on pluginPort
        if(pluginPort<=0 || pluginPort>65535)
        {
            throw new UbikException("Rpc Port number must be between 1 and 65535");
        }
        
        if(hostPort<=0 || hostPort>65535)
        {
            throw new UbikException("Host Port number must be between 1 and 65535");
        }
        
        if(pluginName==null)
        {
            throw new UbikException("Plugin name cannot be null");
        }
        
        while (true)
        {
            try
            {
                var server = new Server
                {
                    Services = {Plugin.BindService(new PluginServiceImpl()) },
                    Ports = { new ServerPort("localhost", pluginPort, ServerCredentials.Insecure) }
                };
                server.Start();
                break;
            }
            catch (IOException)
            {
                pluginPort++;//port is occupied, try next port
                if (pluginPort > 65535)
                {
                    throw new UbikException("Port number must be between 1 and 65535");
                }
            }
        }
        
        //Handshake with host
        var channel = new Channel("localhost:"+hostPort, ChannelCredentials.Insecure);
        var client = new Host.HostClient(channel);
        var reply = client.Handshake(new HandshakeMessage { Name = pluginName, Port = pluginPort });
        if (reply.State != (int)States.Success)
        {
            throw new UbikException("Handshake with host failed, state: "+reply.State);
        }
        
        //Start a ping thread
        Task.Run(() => Ping(client));
    }

    //Ping host every 30s+-5s
    private async Task Ping(Host.HostClient client)
    {
        while (true)
        {
            var random = new Random();
            await Task.Delay(30000 + random.Next(-5000, 5000));//30s +- 5s for each ping, to avoid ping storm
            var reply = client.Ping(new PingMessage());
            if (reply.State != (int)States.Success)
            {
                throw new UbikException("Ping failed, state: "+reply.State);
            }
        }
    }

    private class PluginServiceImpl : Plugin.PluginBase
    {
        public override Task<Response> CreateNode(CreateNodeRequest request, ServerCallContext context)
        {
            try{
                _clientHandel.HandelCreateNode(request);
            }
            catch(Exception e)
            {
                return Task.FromResult(new Response { Status = 500 });//TODO
            }
            
            return Task.FromResult(new Response { Status = 200 });//TODO
        }

        //Recevie message from host
        public override Task<Response> SendMessage(SendMessageRequest request, ServerCallContext context)
        {
            try
            {
                _clientHandel.HandelArriveMessage(request);
            }
            catch (Exception e)
            {
                return Task.FromResult(new Response { Status = 500 });//TODO
            }
            
            return Task.FromResult(new Response { Status = 200 });//TODO
        }

        public override Task<Response> Stop(StopRequest request, ServerCallContext context)
        {
            try
            {
                _clientHandel.HandelStop(request);
            }
            catch (Exception e)
            {
                return Task.FromResult(new Response { Status = 500 });//TODO
            }
            
            return Task.FromResult(new Response { Status = 200 });//TODO
        }

        public override Task<Response> Run(RunRequest request, ServerCallContext context)
        {
            try
            {
                _clientHandel.HandelRun(request);
            }
            catch (Exception e)
            {
                return Task.FromResult(new Response { Status = 500 });//TODO
            }
            
            return Task.FromResult(new Response { Status = 200 });//TODO
        }
    }
}

public interface IClientHandel
{
    void HandelCreateNode(CreateNodeRequest request);
    void HandelArriveMessage(SendMessageRequest request);
    void HandelStop(StopRequest request);
    void HandelRun(RunRequest request);
}

public enum States
{
    Success = 200,
}