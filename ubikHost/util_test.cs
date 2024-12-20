using NUnit.Framework;

namespace ubikHost;

[TestFixture]
public class UbikLoggerTests
{
    [Test]
    public void TestLogPrint()
    {
        var logger = new UbikUtil.UbikLogger(UbikUtil.UbikLogger.DebugLevel, false, "");
        logger.Debug("test debug");
        logger.Info("test info");
        logger.Warn("test warn");
        logger.Error(new Exception("test error"));
        logger.Fatal(new Exception("test fatal"));

        TestContext.Out.WriteLine("Completed TestLogPrint");
    }

    [Test]
    public void TestLogSave()
    {
        var logger = new UbikUtil.UbikLogger(5, true, "./");
        logger.Debug("test debug");
        logger.Info("test info");
        logger.Warn("test warn");
        logger.Error(new Exception("test error"));
        logger.Fatal(new Exception("test fatal"));

        TestContext.Out.WriteLine("Completed TestLogSave");
    }
}

[TestFixture]
public class UbikExceptionTests
{
    [Test]
    public void TestNewUbikException()
    {
        var ex = new UbikUtil.UbikException("an error occurred");
        TestContext.WriteLine(ex.ToString());
        TestContext.WriteLine(ex.ErrorMessage);
    }
}