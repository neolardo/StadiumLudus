using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents an indexable circular buffer.
/// </summary>
/// <typeparam name="T">The type of the buffer items.</typeparam>
public class CircularBuffer<T> :  IEnumerable<T>
{
    #region Properties and Fields
    protected T[] ArrayBuffer { get; }
    public int Size { get; }
    public T this[int index] { get => ArrayBuffer[(HeadIndex - index)%Size]; set => ArrayBuffer[(HeadIndex - index) % Size] = value; }

    protected int HeadIndex = 0;

    #endregion

    #region Methods

    public T GetNext()
    {
        var item = this[0];
        HeadIndex = (HeadIndex + 1) % Size;
        return item;
    }

    public virtual void Clear()
    {
        for (int i = 0; i < Size; i++)
        {
            ArrayBuffer[i] = default;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ArrayBuffer.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ArrayBuffer.GetEnumerator();
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
    /// </summary>
    /// <param name="size">The size of the circular buffer.</param>
    public CircularBuffer(int size)
    {
        Size = size;
        ArrayBuffer = new T[Size];
    }

    #endregion
}
