using System.Diagnostics;
using System.Reflection;
using CSharp.Utils.Extensions;
using NUnit.Framework;
using Speculator.Core;

namespace UnitTests;

[TestFixture]
public class ZexDocTests
{
    private static DirectoryInfo m_projectDir;

    private static DirectoryInfo SnapshotDir => ProjectDir.GetDir("Snapshots");

    public static IEnumerable<string> SnapshotNames { get; } = SnapshotDir.EnumerateFiles("*.json").Select(o => o.Name);
    
    [Test, Explicit]
    public void TestBuilder()
    {
        RunZexTest();
    }

    private static DirectoryInfo ProjectDir
    {
        get
        {
            if (m_projectDir == null)
            {
                var location = Assembly.GetExecutingAssembly().Location;
                m_projectDir = new FileInfo(location).Directory;
                while (m_projectDir?.EnumerateFiles("*.csproj").Any() != true && m_projectDir?.Parent != null)
                    m_projectDir = m_projectDir.Parent;
            }
            
            return m_projectDir;
        }
    }

    [Test, Sequential, Parallelizable(ParallelScope.All)]
    public void TestRunner([ValueSource(nameof(SnapshotNames))] string snapName)
    {
        Assert.That(RunZexTest(SnapshotDir.GetFile(snapName)), Is.True);
    }

    /// <summary>
    /// Run all, or a single, ZexDoc test.
    /// </summary>
    private static bool RunZexTest(FileInfo snapshotFile = null)
    {
        Assert.That(ProjectDir, Is.Not.Null);
        var zexDocBin = ProjectDir.GetDir("Zex").GetFile("zexdoc.com");
        Assert.That(zexDocBin, Does.Exist);

        // Create the CPU.
        using var portHandler = new ZxPortHandler(null, null);
        var cpu = new CPU(new Memory(null), portHandler)
        {
            TheRegisters =
            {
                PC = 0x0100,
                SP = 0xF000
            }
        };

        // Load the ZexDoc image.
        cpu.MainMemory.Poke(cpu.TheRegisters.PC, zexDocBin.ReadAllBytes());

        var baseMemorySnapshot = cpu.MainMemory.Data.ToArray();
        var restoredFromSnapshot = false;
        if (snapshotFile != null)
        {
            // Restore state from a snapshot.
            var snapshot = CpuSnapshot.FromString(snapshotFile.ReadAllText());
            snapshot.ApplyTo(cpu);
            restoredFromSnapshot = true;
        }

        // Run the CPU test(s).
        var makeSnapshotOnConsoleWrite = false;
        var testsFinished = false;
        var testName = string.Empty;
        var didPass = false;
        while (!cpu.IsHalted && !testsFinished)
        {
            // Take a snapshot?
            if (makeSnapshotOnConsoleWrite && !restoredFromSnapshot)
            {
                makeSnapshotOnConsoleWrite = false;
                var snapshot = CpuSnapshot.From(testName, cpu.TheRegisters, baseMemorySnapshot, cpu.MainMemory.Data);
                var snapFile = SnapshotDir.GetFile($"{testName.ToSafeFileName()}.json");
                snapFile.Directory?.Create();
                snapFile.WriteAllText(snapshot.AsString());
            }

            cpu.Step();

            // Console output callback requested?
            if (cpu.TheRegisters.PC != 0x0005)
                continue;
            
            switch (cpu.TheRegisters.Main.C)
            {
                case 0x02: // Single character.
                    Console.Write((char)cpu.TheRegisters.Main.E);
                    Debug.Write((char)cpu.TheRegisters.Main.E);
                    
                    if (cpu.TheRegisters.Main.E is 0x0d or 0x0a)
                        makeSnapshotOnConsoleWrite = true;
                    break;
                    
                case 0x09: // String of characters.
                    var addr = cpu.TheRegisters.Main.DE;
                    testName = string.Empty;
                    char ch;
                    while ((ch = (char)cpu.MainMemory.Peek(addr++)) != '$')
                        testName += ch;

                    testName = testName.Trim();
                    Console.WriteLine(testName);
                    Debug.WriteLine(testName);
                    
                    if (!testName.StartsWith("Z80") && testName.Length > 2 && !testName.Contains(":") && !testName.Contains("complete"))
                        makeSnapshotOnConsoleWrite = true;
                    if (testName.Contains("complete"))
                        testsFinished = true;
                    if (restoredFromSnapshot && (testName.Contains("OK") || testName.Contains(':')))
                    {
                        // We have a test result - Stop.
                        testsFinished = true;
                    }

                    if (testName.Contains("OK"))
                        didPass = true;
                    
                    testName = testName.TrimEnd('.');
                    break;
                    
                default:
                    Assert.Fail("Unexpected output request.");
                    break;
            }
                
            // RET
            cpu.TheRegisters.PC = cpu.MainMemory.PeekWord(cpu.TheRegisters.SP);
            cpu.TheRegisters.SP += 2;
        }

        return didPass;
    }
}