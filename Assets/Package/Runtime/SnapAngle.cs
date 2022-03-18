using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT
{
    public static class SnapAngle
    {
        public static Vector2 Snap(float horizontal, float vertical, int devide)
        {
            var snappedAngle = GetSnappedAngle(horizontal, vertical, devide, out _);
            var magnitude = Mathf.Sqrt(horizontal * horizontal + vertical * vertical);
            return new Vector2(
                Mathf.Cos(snappedAngle) * magnitude,
                 Mathf.Sin(snappedAngle) * magnitude);
        }

        public static Vector2 SnapTo8(float horizontal, float vertical)
        {
            return Snap(horizontal, vertical, 8);
        }

        public static Vector2 SnapTo16(float horizontal, float vertical)
        {
            return Snap(horizontal, vertical, 16);
        }

        public static float GetSnappedAngle(float x, float y, int devide, out int index)
        {
            // var unitAngle = Mathf.PI * 2f / devide;

            var angle = Mathf.Atan2(y, x);

            index = Mathf.RoundToInt(angle * devide / 2f / Mathf.PI);
            var result = index * 2f * Mathf.PI / devide;
            index += devide;
            index %= devide;

            return result;
        }
    }
}
