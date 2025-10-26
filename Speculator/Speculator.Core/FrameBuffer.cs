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

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Speculator.Core;

public static class FrameBuffer
{
    private static readonly Vector3 V255 = new Vector3(255);
    
    public static void SetPixel(Span<byte> framePtr, int stride, int x, int y, Vector3 rgb)
    {
        rgb = Vector3.Clamp(rgb, Vector3.Zero, V255);
        var offset = y * stride + x * 4;
        framePtr[offset] = (byte)rgb.X;
        framePtr[offset + 1] = (byte)rgb.Y;
        framePtr[offset + 2] = (byte)rgb.Z;
        framePtr[offset + 3] = 0xFF;
    }

    /// <summary>
    /// Set a vertical strip of 4 pixels the same RGB color.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetPixelV4(Span<byte> framePtr, int stride, int x, int y, Vector3 rgb, Vector3 scanline)
    {
        // Clamp and pack to a 32-bit BGRA.
        var clamped = Vector3.Clamp(rgb, Vector3.Zero, V255);
        var r = (byte)clamped.X;
        var g = (byte)clamped.Y;
        var b = (byte)clamped.Z;

        // Pack as [A B G R] little-endian in one 32-bit write (alpha forced to 0xFF).
        var pixel = (uint)(r | (g << 8) | (b << 16) | (0xFFu << 24));
        var offset = y * stride + x * 4;

        // First 3 vertical pixels share the same RGB.
        for (var i = 0; i < 3; i++)
        {
            // Unaligned-friendly 32-bit write.
            MemoryMarshal.Write(framePtr.Slice(offset, 4), ref pixel);
            offset += stride;
        }

        // Adjust color for scanline effect and set the 4th pixel.
        clamped *= scanline;
        
        // Re-clamp after multiplication to stay within [0,255].
        var r4 = (byte)Math.Clamp(clamped.X, 0.0f, 255.0f);
        var g4 = (byte)Math.Clamp(clamped.Y, 0.0f, 255.0f);
        var b4 = (byte)Math.Clamp(clamped.Z, 0.0f, 255.0f);
        var pixel4 = (uint)(r4 | (g4 << 8) | (b4 << 16) | (0xFFu << 24));
        MemoryMarshal.Write(framePtr.Slice(offset, 4), ref pixel4);
    }
}
