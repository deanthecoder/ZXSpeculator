using Silk.NET.OpenGL;

namespace CSharp.Utils.OpenGL;

public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    private uint m_handle;
    private BufferTargetARB m_bufferType;
    private GL m_gl;

    public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
    {
        m_gl = gl;
        m_bufferType = bufferType;
        //Clear existing error code.
        GLEnum error;
        do error = m_gl.GetError();
        while (error != GLEnum.NoError);
        m_handle = m_gl.GenBuffer();
        Bind();
        GlErrorException.ThrowIfError(gl);
        fixed (void* d = data)
        {
            m_gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
        GlErrorException.ThrowIfError(gl);
    }

    public void Bind()
    {
        m_gl.BindBuffer(m_bufferType, m_handle);
    }

    public void Dispose()
    {
        m_gl.DeleteBuffer(m_handle);
    }
}