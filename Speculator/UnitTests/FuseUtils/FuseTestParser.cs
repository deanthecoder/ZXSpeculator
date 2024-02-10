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

using CSharp.Core.Extensions;

namespace UnitTests.FuseUtils;

/// <summary>
/// Parses a folder containing Fuse test and result data.
/// </summary>
public class FuseTestParser
{
    private readonly FileInfo m_inFile;
    private readonly FileInfo m_expectedFile;

    public FuseTestParser(DirectoryInfo testDataDir)
    {
        m_inFile = testDataDir.GetFile("tests.in");
        m_expectedFile = testDataDir.GetFile("tests.expected");
    }
    
    public IEnumerable<FuseTest> GetTests()
    {
        var inLines = m_inFile.ReadAllLines().Where(o => !string.IsNullOrWhiteSpace(o)).ToArray();
        var i = 0;
        while (i < inLines.Length)
        {
            var testId = inLines[i++];
            var registers = inLines[i++]; // AF BC DE HL AF' BC' DE' HL' IX IY SP PC
            var state = inLines[i++];     // I R IFF1 IFF2 IM <halted> <tstates>

            // <start address> <byte1> <byte2> ... -1
            var memory = string.Empty;
            string s;
            while ((s = inLines[i++].Trim()) != "-1")
                memory += $"\n{s}";

            yield return new FuseTest(testId, registers, state, memory);
        }
    }

    public IEnumerable<FuseResult> GetResults()
    {
        var eventTypes = new[]
        {
            "MR", "MW", "MC", "PR", "PW", "PC"
        };

        var inLines = m_expectedFile.ReadAllLines().Where(o => !string.IsNullOrWhiteSpace(o)).ToArray();
        var i = 0;
        while (i < inLines.Length)
        {
            var testId = inLines[i++];

            // Events.
            while (eventTypes.Any(o => inLines[i].Contains(o)))
                i++;
            
            var registers = inLines[i++]; // AF BC DE HL AF' BC' DE' HL' IX IY SP PC
            var state = inLines[i++];     // I R IFF1 IFF2 IM <halted> <tstates>
            
            // <start address> <byte1> <byte2> ... -1
            var memory = string.Empty;
            string s;
            while (i < inLines.Length && (s = inLines[i].Trim()).EndsWith("-1"))
            {
                memory += $"\n{s}";
                i++;
            }

            yield return new FuseResult(testId, registers, state, memory);
        }
    }
}