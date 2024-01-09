// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using CSharp.Utils.Extensions;
using CSharp.Utils.UnitTesting;
using NSubstitute;
using NUnit.Framework;
using Speculator.Core;
using UnitTests.FuseUtils;

namespace UnitTests;

[TestFixture]
public class FuseTests : TestsBase
{
    private static FuseTestParser TestParser => new FuseTestParser(ProjectDir.GetDir("FuseTestData"));
    private IPortHandler m_portHandler;

    public static IEnumerable<FuseTest> TheTests { get; } = TestParser.GetTests().ToArray();
    private static IEnumerable<FuseResult> TheResults { get; } = TestParser.GetResults().ToArray();

    [SetUp]
    public void SetUp()
    {
        m_portHandler = Substitute.For<IPortHandler>();
        m_portHandler.In(Arg.Any<ushort>()).Returns(info => (byte)(info.Arg<ushort>() >> 8));
    }

    [Test, Sequential, Parallelizable(ParallelScope.All)]
    public void TestRunner([ValueSource(nameof(TheTests))] FuseTest fuseTest)
    {
        var fuseResult = TheResults.First(o => o.TestId == fuseTest.TestId);

        var cpu = new CPU(new Memory(), m_portHandler);
        fuseTest.InitCpu(cpu);
        Assert.That(fuseTest.Run(cpu, fuseResult.ExpectedPC), Is.True, "Test timed-out.");

        fuseResult.Verify(cpu);
    }
}