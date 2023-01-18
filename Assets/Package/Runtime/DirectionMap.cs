using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public class DirectionMap<T> : IEnumerable<KeyValuePair<Vector2Int, T>>
    {
        public T Right { get; set; }
        public T Left { get; set; }
        public T Up { get; set; }
        public T Down { get; set; }

        public T this[Vector2Int direction]
        {
            get
            {
                return this[direction.x, direction.y];
            }
            set
            {
                this[direction.x, direction.y] = value;
            }
        }

        public T this[int x, int y]
        {
            get
            {
                if (y == 0)
                {
                    if (x == -1)
                    {
                        return Left;
                    }
                    if (x == 1)
                    {
                        return Right;
                    }
                }
                else if (x == 0)
                {
                    if (y == -1)
                    {
                        return Down;
                    }
                    if (y == 1)
                    {
                        return Up;
                    }
                }

                return default;
            }
            set
            {
                if (y == 0)
                {
                    if (x == -1)
                    {
                        Left = value;
                        return;
                    }
                    if (x == 1)
                    {
                        Right = value;
                        return;
                    }
                }
                else if (x == 0)
                {
                    if (y == -1)
                    {
                        Down = value;
                        return;
                    }
                    if (y == 1)
                    {
                        Up = value;
                        return;
                    }
                }

                throw new System.ArgumentOutOfRangeException();
            }
        }

        public IEnumerator<KeyValuePair<Vector2Int, T>> GetEnumerator()
        {
            yield return new KeyValuePair<Vector2Int, T>(Vector2Int.right, Right);
            yield return new KeyValuePair<Vector2Int, T>(Vector2Int.left, Left);
            yield return new KeyValuePair<Vector2Int, T>(Vector2Int.up, Up);
            yield return new KeyValuePair<Vector2Int, T>(Vector2Int.down, Down);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}