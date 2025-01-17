using NUnit.Framework;

namespace ubikHost;

public class PluginTest
{
    private const string ConfigPath = "../../../example-config.json";
    [Test]
    public void TestMount()
    {
        var c = new Core(ConfigPath,true);
        
        var idA=c.AddNode("Init");
        var idB=c.AddNode("Increasing");
        var idC=c.AddNode("Add_Print");
        
        c.UpdateEdge(idA,idB,"out","receive");
        c.UpdateEdge(idA,idC,"out","num_A");
        c.UpdateEdge(idB,idC,"out","num_B");
        c.UpdateEdge(idC,idA,"out","receive");

        var e=c.BeforeRunSet();
        if (e!=null)
        {
            Assert.Fail(e.ToString());
        }
        
        c.Run();
        
        Thread.Sleep(10000);
    }

    [Test]
    public void TestRemoveNode()
    {
        var c = new Core(ConfigPath,true);
        
        var idA=c.AddNode("Init");
        var idB=c.AddNode("Increasing");
        var idC=c.AddNode("Add_Print");
        
        c.UpdateEdge(idA,idB,"out","receive");
        c.UpdateEdge(idA,idC,"out","num_A");
        c.UpdateEdge(idB,idC,"out","num_B");
        c.UpdateEdge(idC,idA,"out","receive");
        
        c.RemoveNode(idB);
        var idD=c.AddNode("Init");
        c.UpdateEdge(idD,idC,"out","num_B");
        c.UpdateEdge(idC,idD,"out","receive");
        
        var e=c.BeforeRunSet();
        if (e!=null)
        {
            Assert.Fail(e.ToString());
        }
        
        c.Run();
        
        Thread.Sleep(10000);
    }
}