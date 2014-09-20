using System;
using System.Collections.Generic;

namespace PathFinding
{
    /// <summary>
    /// Represents a heap of objects that are sorted on entry and removal 
    /// based on their comparable values
    /// </summary>
    public class BinaryHeap<T> where T : IComparable<T>
    {
        /// <summary>
        /// Data of type T stored in the heap
        /// </summary>
        List<T> data = new List<T>();

        /// <summary>
        /// Index of the last element in the heap
        /// </summary>
        int lastItem
        {
            get { return data.Count - 1; }
        }

        /// <summary>
        /// Number of items in the heap
        /// </summary>
        public int Count
        {
            get { return data.Count; }
        }

        /// <summary>
        /// Adds an item to the heap
        /// </summary>
        public void Add(T item)
        {
            data.Add(item);

            // set item for scoring
            SinkDown(lastItem);
        }

        /// <summary>
        /// Removes and returns the last item from the heap
        /// </summary>
        public T Pop()
        {
            // first item in the heap
            T first = data[0];

            // last item in the heap
            T last = data[lastItem];

            data.RemoveAt(lastItem);

            // if there are any elements left, put the last element at the
            // start and let it bubble up
            if (data.Count > 0)
            {
                data[0] = last;

                BubbleUp(0);
            }

            return first;
        }

        /// <summary>
        /// Removes an item from the heap
        /// </summary>
        /// <param name="item"> Item to be removed </param>
        public void Remove(T item)
        {
            T end = data[lastItem];

            data.RemoveAt(lastItem);

            int i = data.IndexOf(item);

            if (i != lastItem)
            {
                if (data[i].CompareTo(item) < 0)
                    SinkDown(i);
                else
                    BubbleUp(i);
            }
        }

        /// <summary>
        /// Rescores an item in the heap
        /// </summary>
        /// <param name="item"> Item to be rescored </param>
        public void RescoreItem(T item)
        {
            SinkDown(data.IndexOf(item));
        }

        /// <summary>
        /// Sinks an item down the heap
        /// </summary>
        /// <param name="i"> Item index in the heap </param>
        public void SinkDown(int i)
        {
            // element to be sunk
            T element = data[i];

            // when at 0, an element can not sink any further
            while (i > 0)
            {
                // compute the parent element's index
                int pIndex = ((i + 1) >> 1) - 1;

                T parent = data[pIndex];

                // swap the elements if the parent is greater, otherwise
                // stop sinking
                if (element.CompareTo(parent) < 0)
                {
                    data[pIndex] = element;
                    data[i] = parent;

                    // update n to continue to the next position
                    i = pIndex;
                }
                else break;
            }
        }

        /// <summary>
        /// Bubbles up an item in the heap
        /// </summary>
        /// <param name="i"> Item index in the heap </param>
        public void BubbleUp(int i)
        {
            // element to be bubbled up
            T element = data[i];

            while (true)
            {
                // indices of the child elements
                int ch2 = (i + 1) << 1,
                    ch1 = ch2 - 1;

                // temporary swap index
                int swap = -1;

                // make sure the index for child one is inside data and
                // compare it to the target element - if it's less than, swap
                // with that index
                if (ch1 < data.Count && data[ch1].CompareTo(element) < 0)
                    swap = ch1;

                // element to compare to depending on if child one was swapped
                T compare = (swap == -1) ? element : data[ch1];

                // do the same for child two
                if (ch2 < data.Count && data[ch2].CompareTo(compare) < 0)
                    swap = ch2;

                // swap elements if needed, otherwise stop bubbling
                if (swap != -1)
                {
                    data[i] = data[swap];
                    data[swap] = element;

                    // set next index to be examined as what was swapped
                    i = swap;
                }
                else break;
            }
        }
    }
}
