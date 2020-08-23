using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TSKT
{
    public class WeightedRandom<T>
    {
        List<T> Values { get; set; }
        List<float> weights;
        float[] selectKeys;

        public float TotalWeight
        {
            get
            {
                if (selectKeys == null)
                {
                    Refresh();
                }
                if (selectKeys.Length == 0)
                {
                    return 0f;
                }
                return selectKeys[selectKeys.Length - 1];
            }
        }
        public int Count
        {
            get
            {
                if (Values == null)
                {
                    return 0;
                }
                return Values.Count;
            }
        }
        public void Add(float weight, T t)
        {
            if (weight < 0f)
            {
                throw new System.Exception();
            }
            if (weight > 0f)
            {
                selectKeys = null;
                if (weights == null)
                {
                    weights = new List<float>
                    {
                        weight
                    };

                    Values = new List<T>();
                }
                else
                {
                    weights.Add(weight);
                }
                Values.Add(t);
            }
        }

        void Remove(int index)
        {
            Values.RemoveAt(index);
            weights.RemoveAt(index);
            selectKeys = null;
        }

        void Refresh()
        {
            selectKeys = new float[weights.Count];
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

            var r = Random.Range(0f, selectKeys[selectKeys.Length - 1]);
            var i = System.Array.BinarySearch(selectKeys, r);
            if (i < 0)
            {
                i = ~i;

            }
            return i;
        }

        public T Select()
        {
            return Values[SelectIndex()];
        }

        public T SelectAndRemove()
        {
            var index = SelectIndex();
            var result = Values[index];
            Remove(index);
            return result;
        }

        public float SearchProbability(T t)
        {
            var i = Values.IndexOf(t);
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

                return weights[i] / selectKeys[selectKeys.Length - 1];
            }
        }
    }
}
