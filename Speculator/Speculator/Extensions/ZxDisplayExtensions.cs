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

using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using CSharp.Core;
using Speculator.Core;

namespace Speculator.Extensions;

public static class ZxDisplayExtensions
{
    public static void SaveAs(this ZxDisplay display, FileInfo pngFile)
    {
        try
        {
            var targetSize = new PixelSize((int)(display.Bitmap.PixelSize.Width * 4.0 / 3.0), display.Bitmap.PixelSize.Height);
            using var scaledBitmap = new RenderTargetBitmap(targetSize);
            using (var ctx = scaledBitmap.CreateDrawingContext())
                ctx.DrawImage(display.Bitmap, new Rect(0, 0, targetSize.Width, targetSize.Height));

            scaledBitmap.Save(pngFile.FullName);
        }
        catch (Exception e)
        {
            Logger.Instance.Exception("Failed to save scaled screenshot.", e);
        }
    }
}