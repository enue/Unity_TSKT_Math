using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public static class Vector2Util
    {
        static public Vector2 Rotate(Vector2 source, float radian)
        {
            var sin = Mathf.Sin(radian);
            var cos = Mathf.Cos(radian);

            return new Vector2(
                cos * source.x - sin * source.y,
                sin * source.x + cos * source.y);
        }
    }
}