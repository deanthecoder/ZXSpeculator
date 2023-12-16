// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If modified, please retain this copyright header, and consider telling us
// about your changes.  We're always glad to see how people use our code!
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.
// We do not accept any liability for damage caused by executing
// or modifying this code.

using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Speculator.Core;

public class ZxDisplay
{
    internal const int ScreenBase = 0x4000;
    private const int ColorMapBase = 0x5800;

    private static readonly List<Color> Colors = new List<Color>
    {
        Color.FromRgb(0x00, 0x00, 0x00),  // Black
        Color.FromRgb(0x00, 0x00, 0xCD),  // Blue
        Color.FromRgb(0xCD, 0x00, 0x00),  // Red
        Color.FromRgb(0xCD, 0x00, 0xCD),  // Magenta
        Color.FromRgb(0x00, 0xCD, 0x00),  // Green
        Color.FromRgb(0x00, 0xCD, 0xCD),  // Cyan
        Color.FromRgb(0xCD, 0xCD, 0x00),  // Yellow
        Color.FromRgb(0xCD, 0xCD, 0xCD),  // White (/Gray)
        Color.FromRgb(0x00, 0x00, 0x00),  // (Bright) Black
        Color.FromRgb(0x00, 0x00, 0xFF),  // Bright Blue
        Color.FromRgb(0xFF, 0x00, 0x00),  // Bright Red
        Color.FromRgb(0xFF, 0x00, 0xFF),  // Bright Magenta
        Color.FromRgb(0x00, 0xFF, 0x00),  // Bright Green
        Color.FromRgb(0x00, 0xFF, 0xFF),  // Bright Cyan
        Color.FromRgb(0xFF, 0xFF, 0x00),  // Bright Yellow
        Color.FromRgb(0xFF, 0xFF, 0xFF)   // Bright White
    };

    private const int LeftMargin = 32;
    private const int RightMargin = 32;
    private const int TopMargin = 24;
    private const int BottomMargin = 24;
    private const int WriteableWidth = 256;
    private const int WritableHeight = 192;

    public WriteableBitmap Bitmap { get; } = new WriteableBitmap(new PixelSize(LeftMargin + WriteableWidth + RightMargin, TopMargin + WritableHeight + BottomMargin), new Vector(96, 96), PixelFormat.Rgba8888);
    public event EventHandler Refreshed;

    private void SetContentPixelGroup(ILockedFramebuffer frameBuffer, int characterColumn, int y, byte pixels, byte attr, bool invertColors)
    {
        var x = characterColumn * 8;

        Debug.Assert(x >= 0 && x <= Bitmap.PixelSize.Width);
        Debug.Assert(y >= 0 && y <= Bitmap.PixelSize.Height);

        // Assign the color data to the pixel.
        int penIndex;
        int paperIndex;
        if (invertColors)
            GetColorIndices(attr, out penIndex, out paperIndex);
        else
            GetColorIndices(attr, out paperIndex, out penIndex);

        var offset = 0;
        for (var i = 7; i >= 0; i--)
        {
            var isSet = (pixels & (1 << i)) != 0;
            frameBuffer.SetPixel(x + offset, y, Colors[isSet ? penIndex : paperIndex]);
            offset++;
        }
    }

    private static void GetColorIndices(byte attr, out int paperIndex, out int penIndex)
    {
        paperIndex = (attr >> 3) & 0x07;
        penIndex = attr & 0x07;
    }

    private byte m_previousBorderColor;

    private const int FramesPerFlash = 16;
    private int m_flashFrameCount;
    private bool m_isFlashing;
    
    internal void UpdateScreen(CPU sender)
    {
        var borderAttr = sender.MainMemory.Peek(0x5C48);

        using (var frameBuffer = Bitmap.Lock())
        {
            if (m_previousBorderColor != borderAttr)
            {
                m_previousBorderColor = borderAttr;

                // Draw top/bottom borders.
                for (var x = 0; x < Bitmap.PixelSize.Width / 8; x++)
                {
                    for (var y = 0; y < TopMargin; y++)
                        SetContentPixelGroup(frameBuffer, x, y, 0x00, borderAttr, false);
                    for (var y = 0; y < BottomMargin; y++)
                        SetContentPixelGroup(frameBuffer, x, TopMargin + WritableHeight + y, 0x00, borderAttr, false);
                }

                // Draw left/right borders.
                for (var y = 0; y < WritableHeight; y++)
                {
                    for (var x = 0; x < LeftMargin / 8; x++)
                        SetContentPixelGroup(frameBuffer, x, TopMargin + y, 0x00, borderAttr, false);
                    for (var x = 0; x < RightMargin / 8; x++)
                        SetContentPixelGroup(frameBuffer, ((LeftMargin + WriteableWidth) / 8) + x, TopMargin + y, 0x00, borderAttr, false);
                }
            }

            // Flashing?
            if (m_flashFrameCount++ == FramesPerFlash)
            {
                m_isFlashing = !m_isFlashing;
                m_flashFrameCount = 0;
            }

            // Draw content.
            for (var i = 0; i < 6144; i++)
            {
                var addr = ScreenBase + i;
                var tt = (addr >> 11) & 0x03;
                var lll = (addr >> 8) & 0x07;
                var cr = (addr >> 5) & 0x07;
                var cc = addr & 0x1F;

                var pixelY = (cr * 8) + (tt * 64) + lll;
                var screenByte = sender.MainMemory.Data[addr];

                var characterAttr = sender.MainMemory.Data[ColorMapBase + (((tt * 8) + cr) * 32) + cc];
                var isFlashSet = (characterAttr & 0x80) != 0;
                SetContentPixelGroup(frameBuffer, cc + (LeftMargin / 8), pixelY + TopMargin, screenByte, characterAttr, m_isFlashing && isFlashSet);
            }
        }

        // todo - Only send if content changed.
        Refreshed?.Invoke(this, EventArgs.Empty);
    }
}