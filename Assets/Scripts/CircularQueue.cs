using UnityEngine;
using System.Collections;
//continue fixing up this class so that it isn't arse

/// <summary>
/// Stack of fixed size that loops when full, overwriting the oldest data.
/// </summary>
/// <typeparam name="T"></typeparam>
public class CircularStack<T>
{
    public int m_top;       //next free element index (top of stack)
    public int m_bottom;    //bottom of stack
    public int m_length;
    public T[] m_array;

    /// <summary>
    /// Construct with a defined fixed size.
    /// </summary>
    /// <param name="_length">Fixed size of stack</param>
    public CircularStack(int _length)
    {
        m_length = _length;
        m_top = 0;
        m_bottom = 0;

        m_array = new T[m_length];
    }

    /// <summary>
    /// Reset top and back indicies. Does not change any elements!
    /// </summary>
    public void reset()
    {
        m_top = 0;
        m_bottom = 0;
    }

    /// <summary>
    /// Distance between top and bottom of stack.
    /// </summary>
    /// <returns></returns>
    public int distance()
    {
        return m_top - m_bottom;
    }

    /// <summary>
    /// Get ref to element in stack where index [0] is the back
    /// and index [length-1] is the top
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    public T at(int _index)
    {
        return m_array[offsetIndex(_index)];
    }

    /// <summary>
    /// Get element in stack where [0] is the 
    /// [0]th element in the array (not adjusted by
    /// top and back of stack).
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    public T atAbsolute(int _index)
    { 
        return m_array[_index];
    }

    /// <summary>
    /// Get element where _depth = 1 gets the topmost element. 
    /// Undefined behaviour for negative numbers.
    /// Zero returns the next free element.
    /// </summary>
    /// <param name="_depth"></param>
    /// <returns></returns>
    public T peekAtDepth(int _depth)
    {
        int index = m_top - _depth;

         if (index < 0) index += m_length;
        return m_array[index];
    }

    /// <summary>
    /// Get ref to element at top of stack and move the 
    /// top pointer back one position.
    /// </summary>
    /// <returns></returns>
    public T pop()
    {
        if (--m_top < 0) m_top += m_length;
        return m_array[m_top];
    }

    /// <summary>
    /// Increment top pointer.
    /// </summary>
    public void incrementFront()
    {
        m_top = (m_top + 1) % m_length;
        if (m_top == m_bottom) m_bottom = (m_bottom + 1) % m_length;
    }

    /// <summary>
    /// Assign a value to element at the top of the stack
    /// and increment the top pointer.
    /// </summary>
    /// <param name="_value"></param>
    public void push(T _value)
    {
        m_array[m_top] = _value;
        incrementFront();
    }

    //--------------No public functions below here!-----------------//

    /// <summary>
    /// Set the element at index (offset from base).
    /// </summary>
    /// <param name="_index"></param>
    /// <param name="value"></param>
    private void set(int _index, T value)
    {
        m_array[offsetIndex(_index)] = value;
    }

    /// <summary>
    /// Set the element at an abosulte index (no offset from base).
    /// </summary>
    /// <param name="_index">Absolute index of element</param>
    /// <param name="value"></param>
    private void setAtAbsoulteIndex(int _index, T _value)
    {
        m_array[_index] = _value;
    }

    /// <summary>
    /// Get the index at an offset from the tail.
    /// Undefined behaviour for negative values.
    /// </summary>
    /// <param name="_offset">Offset from tail</param>
    /// <returns></returns>
    private int offsetIndex(int _offset)
    {
        return (m_bottom + _offset) % m_length;
    }
}
