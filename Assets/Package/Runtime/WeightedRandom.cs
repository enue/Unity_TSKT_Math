using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#nullable enable

namespace TSKT
{
    public class WeightedRandom<T>
    {
        readonly List<T> values = new List<T>();
        readonly List<float> weights = new List<float>();
        float[]? selectKeys;

        public float TotalWeight
        {
            get
            {
                if (selectKeys == null)
                {
                    Refresh();
                }
                if (selectKeys!.Length == 0)
                {
                    return 0f;
                }
                return selectKeys[selectKeys.Length - 1];
            }
        }
        public int Count => values.Count;

        public void Add(float weight, T t)
        {
            if (weight < 0f)
            {
                throw new System.Exception();
            }
            if (weight > 0f)
            {
                selectKeys = null;
                weights.Add(weight);
                values.Add(t);
            }
        }

        void Remove(int index)
        {
            values.RemoveAt(index);
            weights.RemoveAt(index);
            selectKeys = null;
        }

        void Refresh()
        {
            if (weights.Count == 0)
            {
                selectKeys = System.Array.Empty<float>();
                return;
            }
            if (selectKeys == null || selectKeys.Length != weights.Count)
            {
                selectKeys = new float[weights.Count];
            }
            for (int i = 0; i < weights.Count; ++i)
            {
                if (i == 0)
                {
                    selectKeys[i] = weights[i];
                }
                else
                {
                    selectKeys[i] = selectKeys[i - 1] + weights[i];
                }
            }
        }

        int SelectIndex()
        {
            if (selectKeys == null)
            {
                Refresh();
            }

            var r = Random.Range(0f, selectKeys![selectKeys.Length - 1]);
            var i = System.Array.BinarySearch(selectKeys, r);
            if (i < 0)
            {
                i = ~i;

            }
            return i;
        }

        public T Select()
        {
            return values[SelectIndex()];
        }

        public T SelectAndRemove()
        {
            var index = SelectIndex();
            var result = values[index];
            Remove(index);
            return result;
        }

        public float SearchProbability(T t)
        {
            var i = values.IndexOf(t);
            if (i < 0)
            {
                return 0f;
            }
            else
            {
                if (selectKeys == null)
                {
                    Refresh();
                }

                return weights[i] / selectKeys![selectKeys.Length - 1];
            }
        }
    }
}
