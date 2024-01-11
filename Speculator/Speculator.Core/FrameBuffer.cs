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

using Avalonia.Media;

namespace Speculator.Core;

public static class FrameBuffer
{
    private unsafe static Span<byte> GetPixel(byte* framePtr, int frameBufferRowBytes, int x, int y)
    {
        const int bytesPerPixel = 4;
        var offset = frameBufferRowBytes * y + bytesPerPixel * x;
        return new Span<byte>(framePtr + offset, bytesPerPixel);
    }

    public unsafe static void SetPixel(byte* framePtr, int framerBufferStride, int x, int y, Color color, double f = 1.0)
    {
        var pixel = GetPixel(framePtr, framerBufferStride, x, y);
        var alpha = color.A / 255.0 * f;
        pixel[0] = (byte)(color.R * alpha);
        pixel[1] = (byte)(color.G * alpha);
        pixel[2] = (byte)(color.B * alpha);
        pixel[3] = color.A;
    }
}
