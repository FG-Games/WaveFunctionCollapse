/// <summary>
/// By Sebastian Lague
/// Episode 04 - heap/Assets/Scripts/Heap.cs
/// https://github.com/SebLague/Pathfinding/blob/master/Episode%2004%20-%20heap/Assets/Scripts/Heap.cs
/// 
/// ! Important: Avoid adding the same heap item more than once.
/// ! Doing so will lead to a stack overflow due to recursive reordering.
/// 
/// * Use Contains(T item) to check if the item is already in the heap,
/// * and UpdateItem(T item) if the item's priority has changed.
/// </summary>

using System;

namespace WaveFunctionCollapse
{
	public class Heap<T> where T : class, IHeapItem<T>
	{

		public T[] Items { get; private set; }
		public int CurrentItemCount { get; private set; }

		public Heap(int maxHeapSize)
		{
			Items = new T[maxHeapSize];
		}

		public void Add(T item)
		{
			item.HeapIndex = CurrentItemCount;
			Items[CurrentItemCount] = item;
			SortUp(item);
			CurrentItemCount++;
		}

		public T RemoveFirst()
		{
			T firstItem = Items[0];
			CurrentItemCount--;
			Items[0] = Items[CurrentItemCount];
			Items[0].HeapIndex = 0;
			SortDown(Items[0]);
			return firstItem;
		}

		public void UpdateItem(T item)
		{
			SortUp(item);
		}

		public int Count
		{
			get
			{
				return CurrentItemCount;
			}
		}

		public bool Contains(T item)
		{
			return Equals(Items[item.HeapIndex], item);
		}

		void SortDown(T item)
		{
			while (true)
			{
				int childIndexLeft = item.HeapIndex * 2 + 1;
				int childIndexRight = item.HeapIndex * 2 + 2;
				int swapIndex = 0;

				if (childIndexLeft < CurrentItemCount)
				{
					swapIndex = childIndexLeft;

					if (childIndexRight < CurrentItemCount)
					{
						if (Items[childIndexLeft].CompareTo(Items[childIndexRight]) < 0)
						{
							swapIndex = childIndexRight;
						}
					}

					if (item.CompareTo(Items[swapIndex]) < 0)
					{
						Swap(item, Items[swapIndex]);
					}
					else
					{
						return;
					}

				}
				else
				{
					return;
				}

			}
		}

		void SortUp(T item)
		{
			int parentIndex = (item.HeapIndex - 1) / 2;

			while (true)
			{
				T parentItem = Items[parentIndex];
				if (item.CompareTo(parentItem) > 0)
				{
					Swap(item, parentItem);
				}
				else
				{
					break;
				}

				parentIndex = (item.HeapIndex - 1) / 2;
			}
		}

		void Swap(T itemA, T itemB)
		{
			Items[itemA.HeapIndex] = itemB;
			Items[itemB.HeapIndex] = itemA;
			int itemAIndex = itemA.HeapIndex;
			itemA.HeapIndex = itemB.HeapIndex;
			itemB.HeapIndex = itemAIndex;
		}
	}

	public interface IHeapItem<T> : IComparable<T> where T : class
	{
		int HeapIndex
		{
			get;
			set;
		}
	}
}