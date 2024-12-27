using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 優先度付きキュー
/// </summary>
/// <typeparam name="T">キューに格納される要素の型</typeparam>
public class PriorityQueue<T> where T : IComparable<T>
{
    private T[] _array; // キューの内部配列
    private int _count; // キュー内の要素数
    private int _capacity; // キューの容量
    private IComparer<T> _comparer; // 要素の比較に使用する IComparer

    /// <summary>
    /// 指定された容量で優先度付きキューを初期化します。
    /// </summary>
    /// <param name="capacity">キューの容量</param>
    /// <param name="comparer">要素の比較に使用する IComparer</param>
    public PriorityQueue(int capacity, IComparer<T> comparer = null)
    {
        _array = new T[capacity];
        _count = 0;
        _capacity = capacity;
        _comparer = comparer ?? Comparer<T>.Default;
    }

    /// <summary>
    /// 要素をキューに追加します。
    /// </summary>
    /// <param name="item">追加する要素</param>
    /// <exception cref="InvalidOperationException">キューが満杯の場合にスローされます。</exception>
    public void Enqueue(T item)
    {
        if (_count >= _capacity)
        {
            throw new InvalidOperationException("Queue is full");
        }
        _array[_count] = item;
        _count++;
        HeapifyUp(_count - 1); // ヒープのプロパティを維持するために上方向にヒープ化
    }

    /// <summary>
    /// キューから最優先の要素を削除して返します。
    /// </summary>
    /// <returns>削除された要素</returns>
    /// <exception cref="InvalidOperationException">キューが空の場合にスローされます。</exception>
    public T Dequeue()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }
        T item = _array[0];
        _count--;
        _array[0] = _array[_count];
        HeapifyDown(0); // ヒープのプロパティを維持するために下方向にヒープ化
        return item;
    }

    /// <summary>
    /// キューの最優先の要素を返します（削除しません）。
    /// </summary>
    /// <returns>最優先の要素</returns>
    /// <exception cref="InvalidOperationException">キューが空の場合にスローされます。</exception>
    public T Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }
        return _array[0];
    }

    /// <summary>
    /// キュー内の要素数を返します。
    /// </summary>
    public int Count { get { return _count; } }

    /// <summary>
    /// キューの容量を返します。
    /// </summary>
    public int Capacity { get { return _capacity; } }

    /// <summary>
    /// 指定された要素がキューに含まれているかどうかを確認します。
    /// </summary>
    /// <param name="item">確認する要素</param>
    /// <returns>要素がキューに含まれている場合は true、それ以外の場合は false</returns>
    public bool Contains(T item)
    {
        for (int i = 0; i < _count; i++)
        {
            if (_array[i].Equals(item))
            {
                return true;
            }
        }
        return false;
    }

    // 全要素を取得
    public T[] ToArray()
    {
        return _array;
    }

    // []でアクセス
    public T this[int index]
    {
        get { return _array[index]; }
    }

    /// <summary>
    /// ヒープのプロパティを維持するために上方向にヒープ化します。
    /// </summary>
    /// <param name="i">ヒープ化を開始するインデックス</param>
    private void HeapifyUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_comparer.Compare(_array[i], _array[parent]) >= 0)
            {
                break;
            }
            Swap(i, parent);
            i = parent;
        }
    }

    /// <summary>
    /// ヒープのプロパティを維持するために下方向にヒープ化します。
    /// </summary>
    /// <param name="i">ヒープ化を開始するインデックス</param>
    private void HeapifyDown(int i)
    {
        while (true)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;
            if (left < _count && _comparer.Compare(_array[left], _array[smallest]) < 0)
            {
                smallest = left;
            }
            if (right < _count && _comparer.Compare(_array[right], _array[smallest]) < 0)
            {
                smallest = right;
            }
            if (smallest == i)
            {
                break;
            }
            Swap(i, smallest);
            i = smallest;
        }
    }

    /// <summary>
    /// 配列内の2つの要素を交換します。
    /// </summary>
    /// <param name="i">交換する最初の要素のインデックス</param>
    /// <param name="j">交換する2番目の要素のインデックス</param>
    private void Swap(int i, int j)
    {
        T temp = _array[i];
        _array[i] = _array[j];
        _array[j] = temp;
    }
}



