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
            return RandomProvider.GetThreadRandom().GenerateShuffledRange(start, count);
        }

        static public T[] GenerateShuffledArray<T>(List<T> list)
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
    }
}
