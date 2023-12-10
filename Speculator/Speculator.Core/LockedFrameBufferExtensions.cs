using Avalonia.Media;
using Avalonia.Platform;

namespace Speculator.Core;

public static class LockedFrameBufferExtensions
{
    public unsafe static Span<byte> GetPixel(this ILockedFramebuffer frameBuffer, int x, int y)
    {
        const int bytesPerPixel = 4;
        var zero = (byte*)frameBuffer.Address;
        var offset = frameBuffer.RowBytes * y + bytesPerPixel * x;
        return new Span<byte>(zero + offset, bytesPerPixel);
    }

    public static void SetPixel(this ILockedFramebuffer frameBuffer, int x, int y, Color color)
    {
        var pixel = frameBuffer.GetPixel(x, y);
        var alpha = color.A / 255.0;
        pixel[0] = (byte)(color.R * alpha);
        pixel[1] = (byte)(color.G * alpha);
        pixel[2] = (byte)(color.B * alpha);
        pixel[3] = color.A;
    }
}