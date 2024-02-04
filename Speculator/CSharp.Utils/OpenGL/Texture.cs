using Silk.NET.OpenGL;

namespace CSharp.Utils.OpenGL;

public class Texture : IDisposable
{
    private uint m_handle;
    private GL m_gl;

    public unsafe Texture(GL gl, Span<byte> data, uint width, uint height)
    {
        //Saving the gl instance.
        m_gl = gl;

        //Generating the opengl handle;
        m_handle = m_gl.GenTexture();
        Bind();

        //We want the ability to create a texture using data generated from code aswell.
        fixed (void* d = &data[0])
        {
            //Setting the data of a texture.
            m_gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            SetParameters();
        }
    }
        
    private void SetParameters()
    {
        //Setting some texture perameters so the texture behaves as expected.
        /*
        m_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.ClampToEdge);
        m_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.ClampToEdge);
        m_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.LinearMipmapLinear);
        m_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Linear);
        m_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        m_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        */
        //Generating mipmaps.
        m_gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        //When we bind a texture we can choose which textureslot we can bind it to.
        m_gl.ActiveTexture(textureSlot);
        m_gl.BindTexture(TextureTarget.Texture2D, m_handle);
    }

    public void Dispose()
    {
        //In order to dispose we need to delete the opengl handle for the texure.
        m_gl.DeleteTexture(m_handle);
    }
}