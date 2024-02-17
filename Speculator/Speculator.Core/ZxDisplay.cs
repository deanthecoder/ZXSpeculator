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
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CSharp.Core.ViewModels;
using OpenTK.Mathematics;
using Vector = Avalonia.Vector;
using Vector3 = System.Numerics.Vector3;

namespace Speculator.Core;

public class ZxDisplay : ViewModelBase
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
    private bool m_isCrt = true;
    private Vector3 m_scanlineMultiplier;
    private float m_phosphorShrink;
    private float m_brightness;
    private readonly Vector3 m_crtSaturation = new Vector3(1.1f, 1.0f, 1.1f);
    private int m_flashFrameCount;
    private bool m_isFlashing;
    private DateTime m_lastFlashTime = DateTime.Now;
    private double m_emulationSpeed;

    /// <summary>
    /// 'Grain' overlay applied to the CRT screen.
    /// </summary>
    private readonly Vector3[][] m_grain;

    /// <summary>
    /// 320x240 Buffer of pixels, each byte a palette index.
    /// </summary>
    private readonly byte[][] m_screenBuffer = CreateScreenBuffer();

    /// <summary>
    /// Bitmap used to contain the RGB pixel data blitted to the screen.
    /// </summary>
    public WriteableBitmap Bitmap { get; } = CreateWriteableBitmap(true); // Expand height for scanlines.

    /// <summary>
    /// Used to prevent unnecessary UI screen refreshes.
    /// </summary>
    private bool m_didPixelsChange;
    
    private static readonly Vector3[] Colors =
    {
        new Vector3(0x00, 0x00, 0x00), // Black
        new Vector3(0x00, 0x00, 0xCD), // Blue
        new Vector3(0xCD, 0x00, 0x00), // Red
        new Vector3(0xCD, 0x00, 0xCD), // Magenta
        new Vector3(0x00, 0xCD, 0x00), // Green
        new Vector3(0x00, 0xCD, 0xCD), // Cyan
        new Vector3(0xCD, 0xCD, 0x00), // Yellow
        new Vector3(0xCD, 0xCD, 0xCD), // White (/Gray)
        new Vector3(0x00, 0x00, 0x00), // (Bright) Black
        new Vector3(0x00, 0x00, 0xFF), // Bright Blue
        new Vector3(0xFF, 0x00, 0x00), // Bright Red
        new Vector3(0xFF, 0x00, 0xFF), // Bright Magenta
        new Vector3(0x00, 0xFF, 0x00), // Bright Green
        new Vector3(0x00, 0xFF, 0xFF), // Bright Cyan
        new Vector3(0xFF, 0xFF, 0x00), // Bright Yellow
        new Vector3(0xFF, 0xFF, 0xFF)  // Bright White
    };
    
    public byte BorderAttr { get; set; }

    public bool IsCrt
    {
        get => m_isCrt;
        set
        {
            m_isCrt = value;
            m_didPixelsChange = true;

            if (m_isCrt)
            {
                m_scanlineMultiplier = new Vector3(0.7f);
                m_phosphorShrink = 0.5f;
            }
            else
            {
                m_scanlineMultiplier = Vector3.One;
                m_phosphorShrink = 1.0f;
            }

            m_brightness = 3.0f / (1.0f + 2.0f * m_phosphorShrink);

            var random = new Random(0);
            for (var i = 0; i < m_screenBuffer.Length; i++)
            {
                for (var j = 0; j < m_screenBuffer[0].Length; j++)
                    m_grain[i][j] = m_isCrt ? new Vector3((float)(random.NextDouble() * 12.0)) : Vector3.Zero;
            }
        }
    }

    /// <summary>
    /// The emulation speed (where 1.0 => 100% Speccy).
    /// </summary>
    public double EmulationSpeed
    {
        get => m_emulationSpeed;
        private set => SetField(ref m_emulationSpeed, value);
    }

    public event EventHandler Refreshed;

    public ZxDisplay()
    {
        m_grain = new Vector3[m_screenBuffer.Length][];
        for (var i = 0; i < m_screenBuffer.Length; i++)
            m_grain[i] = new Vector3[m_screenBuffer[0].Length];
    }

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
        var bitmap = CreateWriteableBitmap(false);
        using var frameBuffer = bitmap.Lock();
        unsafe
        {
            var framePtr = (byte*)frameBuffer.Address;
            var ptr = new Span<byte>(framePtr, frameBuffer.RowBytes * frameBuffer.Size.Height);
            for (var y = 0; y < screenBuffer.Length; y++)
            {
                var row = screenBuffer[y];
                for (var x = 0; x < screenBuffer[0].Length; x++)
                    FrameBuffer.SetPixel(ptr, frameBuffer.RowBytes, x, y, Colors[row[x]]);
            }
        }

        return bitmap;
    }

    private unsafe void UpdateScreen()
    {
        using (var frameBuffer = Bitmap.Lock())
        {
            var framerBufferStride = frameBuffer.RowBytes;
            var w = m_screenBuffer[0].Length;
            var h = m_screenBuffer.Length;
            var ptr = new Span<byte>((byte*)frameBuffer.Address, frameBuffer.RowBytes * frameBuffer.Size.Height);

            var phosphorR = new Vector3(1.0f, m_phosphorShrink, m_phosphorShrink);
            var phosphorG = new Vector3(m_phosphorShrink, 1.0f, m_phosphorShrink);
            var phosphorB = new Vector3(m_phosphorShrink, m_phosphorShrink, 1.0f);

            // Software pixel shader.
            for (var y = 0; y < h; y++)
            {
                var row = m_screenBuffer[y];
                var uvY = (double)y / h;

                for (var x = 0; x < w; x++)
                {
                    var origColor = Colors[row[x]];

                    if (IsCrt)
                    {
                        var uvX = (double)x / w;
                        var vignette = (float)MathHelper.Lerp(0.7, 1.0, Math.Sqrt(64.0 * uvX * uvY * (1.0 - uvX) * (1.0 - uvY)));
                        
                        origColor += m_grain[y][x];
                        origColor *= m_brightness * vignette * m_crtSaturation;
                    }
                    
                    var xx = x * 3;
                    var yy = y * 4;
                    FrameBuffer.SetPixelV4(ptr, framerBufferStride, xx, yy, origColor * phosphorR, m_scanlineMultiplier);
                    FrameBuffer.SetPixelV4(ptr, framerBufferStride, xx + 1, yy, origColor * phosphorG, m_scanlineMultiplier);
                    FrameBuffer.SetPixelV4(ptr, framerBufferStride, xx + 2, yy, origColor * phosphorB, m_scanlineMultiplier);
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
    /// <remarks>
    /// The expanded bitmap allows for scanlines and RGB phosphor dots to be added.
    /// </remarks>
    private static WriteableBitmap CreateWriteableBitmap(bool expandForFx) =>
        new WriteableBitmap(new PixelSize((LeftMargin + WriteableWidth + RightMargin) * (expandForFx ? 3 : 1), (TopMargin + WritableHeight + BottomMargin) * (expandForFx ? 4 : 1)), new Vector(96, 96), PixelFormat.Rgba8888);
}
