using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections;

namespace TSKT
{
    static public class RandomUtil
    {
        static public int[] GenerateShuffledRange(int start, int count)
        {
            if (count == 0)
            {
                return System.Array.Empty<int>();
            }

            var result = new int[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = i + start;
            }
            Shuffle(ref result);
            return result;
        }

        static public T[] GenerateShuffledArray<T>(List<T> list)
        {
            if (list.Count == 0)
            {
                return System.Array.Empty<T>();
            }

            var result = new T[list.Count];
            for(int i=0; i<result.Length; ++i)
            {
                result[i] = list[i];
            }
            Shuffle(ref result);
            return result;
        }

        static public void Shuffle<T>(ref List<T> list)
        {
            for(int i=0; i < list.Count - 1; ++i)
            {
                var swapIndex = Random.Range(i, list.Count);
                var t = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = t;
            }
        }

        static public void Shuffle<T>(ref T[] list)
        {
            for (int i = 0; i < list.Length - 1; ++i)
            {
                var swapIndex = Random.Range(i, list.Length);
                var t = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = t;
            }
        }
    }
}
