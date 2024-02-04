using Silk.NET.OpenGL;

namespace CSharp.Utils.OpenGL;

public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    private uint m_handle;
    private GL m_gl;

    public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
    {
        m_gl = gl;

        m_handle = m_gl.GenVertexArray();
        Bind();
        vbo.Bind();
        ebo.Bind();
    }

    public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
    {
        m_gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVertexType), (void*)(offSet * sizeof(TVertexType)));
        m_gl.EnableVertexAttribArray(index);
    }

    public void Bind()
    {
        m_gl.BindVertexArray(m_handle);
    }

    public void Dispose()
    {
        m_gl.DeleteVertexArray(m_handle);
    }
}