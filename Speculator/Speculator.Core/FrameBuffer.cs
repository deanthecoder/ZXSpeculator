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
    public static void SetPixelV4(Span<byte> framePtr, int stride, int x, int y, Vector3 rgb, Vector3 f, Vector3 scanline)
    {
        var clampedRgb = Vector3.Clamp(rgb * f, Vector3.Zero, V255);

        // First 3 vertical pixels.
        var offset = y * stride + x * 4;
        for (var i = 0; i < 3; i++)
        {
            framePtr[offset] = (byte)clampedRgb.X;
            framePtr[offset + 1] = (byte)clampedRgb.Y;
            framePtr[offset + 2] = (byte)clampedRgb.Z;
            framePtr[offset + 3] = 0xFF;
            offset += stride;
        }

        // Adjust color for scanline effect and set the 4th pixel.
        clampedRgb *= scanline;
        framePtr[offset] = (byte)clampedRgb.X;
        framePtr[offset + 1] = (byte)clampedRgb.Y;
        framePtr[offset + 2] = (byte)clampedRgb.Z;
        framePtr[offset + 3] = 0xFF;
    }
}
