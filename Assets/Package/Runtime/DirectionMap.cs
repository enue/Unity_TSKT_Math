using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public class DirectionMap<T> : IEnumerable<(Vector2Int Key, T Value)>
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
                return (x, y) switch
                {
                    (-1, 0) => Left,
                    (1, 0) => Right,
                    (0, -1) => Down,
                    (0, 1) => Up,
                    _ => default
                };
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

        public IEnumerator<(Vector2Int Key, T Value)> GetEnumerator()
        {
            yield return (Vector2Int.right, Right);
            yield return (Vector2Int.left, Left);
            yield return (Vector2Int.up, Up);
            yield return (Vector2Int.down, Down);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}