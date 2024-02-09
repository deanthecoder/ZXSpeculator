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

using K4os.Compression.LZ4;

namespace CSharp.Utils.Extensions;

public static class ByteExtensions
{
    public static byte[] Compress(this byte[] input) =>
        LZ4Pickler.Pickle(input);

    public static byte[] Decompress(this byte[] input) =>
        LZ4Pickler.Unpickle(input);
}