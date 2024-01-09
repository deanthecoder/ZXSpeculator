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

using System.Diagnostics;

namespace Speculator.Core.Extensions;

public static class ByteExtensions
{
    public static bool IsBitSet(this byte b, byte i)
    {
        Debug.Assert(i <= 7, "Index out of range.");
        return (b & (1 << i)) != 0;
    }

    public static byte ResetBit(this byte b, byte i)
    {
        Debug.Assert(i <= 7, "Index out of range.");
        var mask = (byte)~(1 << i);
        return (byte)(b & mask);
    }

    public static byte SetBit(this byte b, byte i)
    {
        Debug.Assert(i <= 7, "Index out of range.");
        return (byte)(b | (1 << i));
    }
}