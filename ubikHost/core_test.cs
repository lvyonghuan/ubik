using NUnit.Framework;

namespace ubikHost;

[TestFixture]
public class CoreTest
{
    private const string ConfigPath = "../../../example-config.json";
    [Test]
    public  void TestInitCore()
    {
        var c = new Core(ConfigPath);
    }
}