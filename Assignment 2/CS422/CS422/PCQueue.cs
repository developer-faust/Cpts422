/* Colin Phillips
 * CS 422 - Assignment 2
 * Fall 2016 */

using System;

namespace CS422
{
	/// <summary>
	/// Producer-Consumer (thread-safe) queue.
	/// </summary>
    public class PCQueue
    {
        private PCQNode _head;
        private PCQNode _tail;
        private PCQNode _dummy;

		/// <summary>
		/// Initializes a new instance of the <see cref="CS422.PCQueue"/> class.
		/// </summary>
        public PCQueue()
        {
            _head = _tail = _dummy = new PCQNode(-1);
        }

		/// <summary>
		/// Enqueue the specified value.
		/// </summary>
		/// <param name="dataValue">The value to enqueue.</param>
        public void Enqueue(int dataValue)
        {
            _tail.Next = new PCQNode(dataValue);
            _tail = _tail.Next;
        }

		/// <summary>
		/// Dequeue the front value from the queue..
		/// </summary>
		/// <param name="out_value">A reference to the int object to place the dequeued value.</param>
        public bool Dequeue(ref int out_value)
        {
            if (ReferenceEquals(_head, _dummy) && ReferenceEquals(_tail, _dummy))
            {
                // Empty queue
                return false;
            }

			out_value = _head.Data;

			if (ReferenceEquals (_head.Next, _tail)) 
			{
				// Only one value in the queue (front ref == dummy ref)
				_head = _tail = _dummy;
			} 
			else 
			{
				_head.Next = _head.Next.Next;
			}

			return true;
        }
    }

	/// <summary>
	/// Producer-Consumer queue node for use in the PCQueue class.
	/// </summary>
    internal class PCQNode
    {
		/// <summary>
		/// Gets the data associated with this node.
		/// </summary>
        public int Data { get; private set; }

		/// <summary>
		/// Gets or sets the value of this node's next (PCQNode) reference.
		/// </summary>
		/// <value>The next.</value>
        public PCQNode Next { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CS422.PCQNode"/> class.
		/// </summary>
		/// <param name="value">The data that this node will contain.</param>
        public PCQNode(int value)
        {
            Data = value;
        }
    }
}
