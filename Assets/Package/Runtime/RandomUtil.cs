using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections;
#nullable enable

namespace TSKT
{
    static public class RandomUtil
    {
        static public int[] GenerateShuffledRange(int start, int count)
        {
            return RandomProvider.GetThreadRandom().GenerateShuffledRange(start, count);
        }

        static public T[] GenerateShuffledArray<T>(IReadOnlyList<T> list)
        {
            return RandomProvider.GetThreadRandom().GenerateShuffledArray(list);
        }

        static public void Shuffle<T>(ref List<T> list)
        {
            RandomProvider.GetThreadRandom().Shuffle(ref list);
        }

        static public void Shuffle<T>(ref T[] list)
        {
            RandomProvider.GetThreadRandom().Shuffle(ref list);
        }

        static public T Sample<T>(T[] array)
        {
            return Sample(array, out _);
        }

        static public T Sample<T>(T[] array, out int index)
        {
            index = Random.Range(0, array.Length);
            return array[index];
        }

        static public T Sample<T>(IReadOnlyList<T> list)
        {
            return Sample(list, out _);
        }

        static public T Sample<T>(IReadOnlyList<T> list, out int index)
        {
            index = Random.Range(0, list.Count);
            return list[index];
        }
    }
}
