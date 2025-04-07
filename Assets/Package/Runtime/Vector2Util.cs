using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
#nullable enable

namespace TSKT
{
    public static class Vector2Util
    {
        public static Vector2 Rotate(Vector2 source, float radian)
        {
            var sin = Mathf.Sin(radian);
            var cos = Mathf.Cos(radian);

            return new Vector2(
                cos * source.x - sin * source.y,
                sin * source.x + cos * source.y);
        }

        public static float2 Rotate(in float2 source, float radian)
        {
            var sin = math.sin(radian);
            var cos = math.cos(radian);

            return new float2(
                cos * source.x - sin * source.y,
                sin * source.x + cos * source.y);
        }
    }
}