using System;

public class Heap<T> where T : IHeapItem<T>
{
    T[] items;
    int itemCount = 0;

    public Heap(int maxSize)
    {
        items = new T[maxSize];
    }
    //accessors
    public int Count 
    {
        get{return itemCount;}
    }
    public void Add(T item)
    {
        item.HeapIndex = itemCount;
        items[itemCount] = item;
        ShortUp(item);
        itemCount++;
    }
    public T Take()
    {
        T item = items[0];
        itemCount--;
        items[0] = items[itemCount];
        items[0].HeapIndex = 0;
        ShortDown(items[0]);
        return item;
    }

    public void UpdateItems(T item)
    {
        ShortUp(item);
    }
    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    //modifier
    void ShortUp(T item)
    {
        int parentID = (int)((item.HeapIndex) * 0.5);
        while(true)
        {
            T parent = items[parentID];
            if(item.CompareTo(parent) > 0)
            {
                Swap(item,parent);
            }
            else break; 
            parentID = (int)((item.HeapIndex) * 0.5);
        }
    }
    void ShortDown(T item)
    {
        while(true)
        {
            int leftChildID = item.HeapIndex * 2 + 1;
            int rightChildID = item.HeapIndex * 2 + 2;
            int tempId = 0;

            if(leftChildID < itemCount)
            {
                tempId = leftChildID;
                if(rightChildID < itemCount)
                {
                    if(items[leftChildID].CompareTo(items[rightChildID]) < 0)
                    {
                        tempId = rightChildID;
                    }
                }
                if(item.CompareTo(items[tempId]) < 0) Swap(item,items[tempId]);
                else return;
            }
            else return;
        }
    }
    void Swap(T a, T b)
    {
        items[a.HeapIndex] = b;
        items[b.HeapIndex] = a;
        int temp = a.HeapIndex;
        a.HeapIndex = b.HeapIndex;
        b.HeapIndex = temp;
    }
}
public interface IHeapItem<T> : IComparable<T>
{
    public int HeapIndex { get; set; }
}

