/* Colin Phillips
 * CS 422 - Assignment 2
 * Fall 2016 */

using System;

namespace CS422
{
  public class PCQueue
  {
    private PCQNode _head;
    private PCQNode _tail;
    private PCQNode _dummy;

    public PCQueue()
    {
      _head = _tail = null;
      _dummy = new PCQNode(-1);
    }

    public void Enqueue(int dataValue)
    {
      _tail.Next = new PCQNode(dataValue);
      _tail = _tail.Next;
    }

    public bool Dequeue(ref int out_value)
    {
      
    }
  }

  internal class PCQNode
  {
    public int Data { get; private set; }

    public PCQNode Next { get; set; }

    public PCQNode(int value)
    {
      Data = value;
    }
  }
}
