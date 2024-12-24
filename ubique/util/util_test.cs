using NUnit.Framework;

namespace ubique.util;

[TestFixture]
public class UbikLoggerTests
{
    [Test]
    public void TestLogPrint()
    {
        var logger = new UbikLogger(UbikLogger.DebugLevel, false, "");
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
        var logger = new UbikLogger(5, true, "./");
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
        var ex = new UbikException("an error occurred");
        TestContext.WriteLine(ex.ToString());
        TestContext.WriteLine(ex.ErrorMessage);
    }
}