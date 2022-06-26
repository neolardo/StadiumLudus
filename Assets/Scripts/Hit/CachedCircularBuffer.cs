using System;
using System.Collections.Generic;

/// <summary>
/// Represents a cached <see cref="CircularBuffer{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
public class CachedCircularBuffer<T> : CircularBuffer<T> where T : new()
{
    #region Methods

    public override void Clear()
    {
        for (int i = 0; i < Size; i++)
        {
            ArrayBuffer[i] = new T();
        }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedCircularBuffer{T}"/> class.
    /// </summary>
    /// <param name="size">The size of the circular buffer.</param>
    public CachedCircularBuffer(int size) : base(size)
    {
        for(int i=0; i< Size; i++)
        {
            ArrayBuffer[i] = new T();
        }
    }

    #endregion
}
