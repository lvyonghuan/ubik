using NUnit.Framework;

namespace ubikHost;

public class RpcTest
{
    private const string ConfigPath = "../../../example-config.json";
    [Test]
    public void TestInit()
    {
        var rpc = new Rpc();
        var port = rpc.Init();
        Console.WriteLine(port);
        //阻塞
        while (true)
        {
            Thread.Sleep(1000);
        }
    }
}