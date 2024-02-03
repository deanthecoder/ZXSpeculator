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
using PixelFormat = Avalonia.Platform.PixelFormat;

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
    private DateTime m_lastFlashTime = DateTime.Now;

    /// <summary>
    /// Buffer of pixels, each byte a palette index.
    /// </summary>
    private readonly byte[][] m_screenBuffer = CreateScreenBuffer();

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

    public WriteableBitmap Bitmap { get; } = CreateWriteableBitmap();

    public byte BorderAttr { get; set; }
    
    /// <summary>
    /// The emulation speed (where 1.0 => 100% Speccy).
    /// </summary>
    public double EmulationSpeed { get; private set; }

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
        var didReachScreenBottom = RenderScanlineIntoBuffer(args.memory, args.scanline, m_screenBuffer, BorderAttr, m_isFlashing, ref m_didPixelsChange);

        // If scanline reached the bottom of the screen, update the UI.
        if (!didReachScreenBottom)
            return;
        
        // Update the flash.
        if (m_flashFrameCount++ == FramesPerFlash)
        {
            m_isFlashing = !m_isFlashing;
            m_flashFrameCount = 0;
            
            // Also update the EmulationSpeed.
            var now = DateTime.Now;
            var elapsedSecs = (now - m_lastFlashTime).TotalSeconds;
            EmulationSpeed = Math.Round(FramesPerFlash / (50.0 * elapsedSecs) / 0.25) * 0.25;
            m_lastFlashTime = now;
        }

        if (m_didPixelsChange)
            UpdateScreen();
        m_didPixelsChange = false;
    }

    /// <summary>
    /// Returns true if the scanline has reached the bottom of the screen.
    /// </summary>
    private static bool RenderScanlineIntoBuffer(Memory memory, int scanlineIndex, byte[][] screenBuffer, byte borderAttr, bool isFlashing, ref bool didPixelsChange)
    {
        var y = scanlineIndex - (48 - TopMargin);
        if (y < 0 || y >= screenBuffer.Length)
        {
            // Off-screen(/vsync) area.
            return false;
        }
        
        // Fill entire line with border color.
        var border = GetColorIndices(borderAttr).Item1;
        if (screenBuffer[y][0] != border)
        {
            didPixelsChange = true;
            Array.Fill(screenBuffer[y], border);
        }

        // Set Y to the drawable screen coordinate.
        y -= TopMargin;
        if (y < 0 || y >= WritableHeight)
        {
            // In top or bottom border area - No screen content needed.
            return false;
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
            var screenByte = memory.Data[srcRowStart + characterColumn]; 

            // Get the pen/paper color for this block.
            var a = characterRow * 32 + characterColumn;
            var attr = memory.Data[ColorMapBase + a];
            var isFlashSet = (attr & 0x80) != 0;
            var penAndPaper = GetColorIndices(attr, isFlashing && isFlashSet);

            // Write indexed colors for the 8 pixel block.
            var off = LeftMargin + characterColumn * 8;
            for (var i = 7; i >= 0; i--, off++)
            {
                var isSet = (screenByte & (1 << i)) != 0;
                var index = isSet ? penAndPaper.Item1 : penAndPaper.Item2;
                if (screenBuffer[y + TopMargin][off] == index)
                    continue;
                didPixelsChange = true;
                screenBuffer[y + TopMargin][off] = index;
            }
        }

        return y == WritableHeight - 1;
    }

    /// <summary>
    /// Create a full screen image from the current state of memory.
    /// </summary>
    public static Bitmap CaptureScreenFromMemory(Memory theMemory, byte borderAttr)
    {
        // Render the screen into a buffer of indexed palette values.
        const int scanlineCount = 312;
        var screenBuffer = CreateScreenBuffer();
        for (var i = 0; i < scanlineCount; i++)
        {
            var unused = false;
            RenderScanlineIntoBuffer(theMemory, i, screenBuffer, borderAttr, false, ref unused);
        }

        // Convert buffer to an image.
        var bitmap = CreateWriteableBitmap();
        using var frameBuffer = bitmap.Lock();
        unsafe
        {
            var framePtr = (byte*)frameBuffer.Address;
            for (var y = 0; y < screenBuffer.Length; y++)
            {
                var row = screenBuffer[y];
                for (var x = 0; x < screenBuffer[0].Length; x++)
                    FrameBuffer.SetPixel(framePtr, frameBuffer.RowBytes, x, y, Colors[row[x]]);
            }
        }

        return bitmap;
    }
    
    private unsafe void UpdateScreen()
    {
        using (var frameBuffer = Bitmap.Lock())
        {
            var framePtr = (byte*)frameBuffer.Address;
            var framerBufferStride = frameBuffer.RowBytes;
            var w = m_screenBuffer[0].Length;
            var h = m_screenBuffer.Length;

            for (var y = 0; y < h; y++)
            {
                var row = m_screenBuffer[y];
                for (var x = 0; x < w; x++)
                {
                    var color = Colors[row[x]];
                    FrameBuffer.SetPixel(framePtr, framerBufferStride, x, y, color);
                }
            }
        }

        Refreshed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Buffer of pixels, each byte a palette index.
    /// </summary>
    private static byte[][] CreateScreenBuffer() =>
        Enumerable.Range(0, TopMargin + WritableHeight + BottomMargin).Select(_ => new byte[LeftMargin + WriteableWidth + RightMargin]).ToArray();

    /// <summary>
    /// Create a bitmap suitable for use in the UI.
    /// </summary>
    /// <returns></returns>
    private static WriteableBitmap CreateWriteableBitmap() =>
        new WriteableBitmap(new PixelSize(LeftMargin + WriteableWidth + RightMargin, TopMargin + WritableHeight + BottomMargin), new Vector(96, 96), PixelFormat.Rgba8888);
}

// todo - extract