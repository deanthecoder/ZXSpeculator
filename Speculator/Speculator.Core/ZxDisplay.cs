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

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Speculator.Core;

public class ZxDisplay
{
    public const int ScreenBase = 0x4000;
    private const int ColorMapBase = 0x5800;
    private const int LeftMargin = 32;
    private const int RightMargin = 32;
    private const int TopMargin = 24;
    private const int BottomMargin = 24;
    private const int WriteableWidth = 256;
    private const int WritableHeight = 192;
    private const int FramesPerFlash = 16;
    private int m_flashFrameCount;
    private bool m_isFlashing;
    private bool m_isCrt = true;

    /// <summary>
    /// Buffer of pixels, each byte a palette index.
    /// </summary>
    private readonly byte[][] m_screenBuffer = Enumerable.Range(0, TopMargin + WritableHeight + BottomMargin).Select(_ => new byte[LeftMargin + WriteableWidth + RightMargin]).ToArray();

    /// <summary>
    /// Used to prevent unnecessary UI screen refreshes.
    /// </summary>
    private bool m_didPixelsChange;
    
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

    public WriteableBitmap Bitmap { get; } = new WriteableBitmap(new PixelSize(LeftMargin + WriteableWidth + RightMargin, (TopMargin + WritableHeight + BottomMargin) * 4), new Vector(96, 96), PixelFormat.Rgba8888);

    public byte BorderAttr { get; set; }

    public bool IsCrt
    {
        get => m_isCrt;
        set
        {
            if (m_isCrt == value)
                return;
            m_isCrt = value;
            m_didPixelsChange = true;
        }
    }

    public event EventHandler Refreshed;

    private static (byte, byte) GetColorIndices(byte attr, bool invert = false)
    {
        var paperIndex = (byte)(attr >> 3 & 0x07);
        var penIndex = (byte)(attr & 0x07);
        
        var isBright = attr & 0x40;
        if (isBright != 0)
        {
            paperIndex += 8;
            penIndex += 8;
        }

        return invert ? (paperIndex, penIndex) : (penIndex, paperIndex);
    }
    
    public void OnRenderScanline(object sender, (Memory memory, int scanline) args)
    {
        var y = args.scanline - (48 - TopMargin);
        if (y < 0 || y >= m_screenBuffer.Length)
        {
            // Off-screen(/vsync) area.
            return;
        }
        
        // Fill entire line with border color.
        var border = GetColorIndices(BorderAttr).Item1;
        if (m_screenBuffer[y][0] != border)
        {
            m_didPixelsChange = true;
            Array.Fill(m_screenBuffer[y], border);
        }

        // Set Y to the drawable screen coordinate.
        y -= TopMargin;
        if (y < 0 || y >= WritableHeight)
        {
            // In top or bottom border area - No screen content needed.
            return;
        }
        
        // Draw screen pixel content.
        var y76 = (byte)(y >> 6);
        var y210 = (byte)(y & 0x07);
        var y543 = (byte)((y >> 3) & 0x07);
        var srcRowStart = (ushort)(0x4000 | (y76 << 11) | (y210 << 8) | (y543 << 5));

        var characterRow = y / 8;
        for (var characterColumn = 0; characterColumn < 32; characterColumn++)
        {
            // Get block of 8 horizontal pixels.
            var screenByte = args.memory.Data[srcRowStart + characterColumn]; 

            // Get the pen/paper color for this block.
            var a = characterRow * 32 + characterColumn;
            var attr = args.memory.Data[ColorMapBase + a];
            var isFlashSet = (attr & 0x80) != 0;
            var penAndPaper = GetColorIndices(attr, m_isFlashing && isFlashSet);

            // Write indexed colors for the 8 pixel block.
            var off = LeftMargin + characterColumn * 8;
            for (var i = 7; i >= 0; i--, off++)
            {
                var isSet = (screenByte & (1 << i)) != 0;
                var index = isSet ? penAndPaper.Item1 : penAndPaper.Item2;
                if (m_screenBuffer[y + TopMargin][off] == index)
                    continue;
                m_didPixelsChange = true;
                m_screenBuffer[y + TopMargin][off] = index;
            }
        }
        
        // If scanline reached the bottom of the screen, update the UI.
        if (y == WritableHeight - 1)
        {
            // Update the flash.
            if (m_flashFrameCount++ == FramesPerFlash)
            {
                m_isFlashing = !m_isFlashing;
                m_flashFrameCount = 0;
            }
            
            if (m_didPixelsChange)
                UpdateScreen();
            m_didPixelsChange = false;
        }
    }
    
    private unsafe void UpdateScreen()
    {
        using (var frameBuffer = Bitmap.Lock())
        {
            var framePtr = (byte*)frameBuffer.Address;
            var framerBufferStride = frameBuffer.RowBytes;
            var w = m_screenBuffer[0].Length;
            var h = m_screenBuffer.Length;

            if (IsCrt)
            {
                // Software pixel shader.
                for (var y = 0; y < h; y++)
                {
                    var row = m_screenBuffer[y];
                    var v = (y - h * 0.5) / h;
                    v *= v;
                    for (var x = 0; x < w; x++)
                    {
                        var color = Colors[row[x]];
                        var u = (x - w * 0.5) / w;
                        var vignette = u * u + v;
                        vignette *= vignette * vignette;
                        vignette = 1.0 - vignette * 2.0;
                        FrameBuffer.SetPixel(framePtr, framerBufferStride, x, y * 4, color, vignette);
                        FrameBuffer.SetPixel(framePtr, framerBufferStride, x, y * 4 + 1, color, vignette);
                        FrameBuffer.SetPixel(framePtr, framerBufferStride, x, y * 4 + 2, color, vignette);
                        FrameBuffer.SetPixel(framePtr, framerBufferStride, x, y * 4 + 3, color, 0.85 * vignette);
                    }
                }
            }
            else
            {
                // Straight blit - No FX.
                for (var y = 0; y < h; y++)
                {
                    var row = m_screenBuffer[y];
                    for (var x = 0; x < w; x++)
                    {
                        var color = Colors[row[x]];
                        FrameBuffer.SetPixel(framePtr, framerBufferStride, x, y * 4, color);
                        FrameBuffer.SetPixel(framePtr, framerBufferStride, x, y * 4 + 1, color);
                        FrameBuffer.SetPixel(framePtr, framerBufferStride, x, y * 4 + 2, color);
                        FrameBuffer.SetPixel(framePtr, framerBufferStride, x, y * 4 + 3, color);
                    }
                }
            }
        }

        Refreshed?.Invoke(this, EventArgs.Empty);
    }
}
