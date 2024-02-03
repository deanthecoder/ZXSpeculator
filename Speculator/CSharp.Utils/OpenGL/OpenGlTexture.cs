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

using OpenTK.Graphics.OpenGL;

namespace CSharp.Utils.OpenGL;

/// <summary>
/// Represents a texture that can be passed into an OpenGL fragment shader.
/// </summary>
public class OpenGlTexture : IDisposable
{
    private readonly int m_handle = GL.GenTexture();
    private bool m_disposedValue;

    public void LoadFrom(int width, int height, IntPtr pixels)
    {
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public void Use(TextureUnit unit = TextureUnit.Texture0)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, m_handle);
    }

    ~OpenGlTexture()
    {
        GL.DeleteTexture(m_handle);
    }

    public void Dispose()
    {
        if (m_disposedValue)
            return;
        m_disposedValue = true;

        GL.DeleteTexture(m_handle);
        GC.SuppressFinalize(this);
    }
}
