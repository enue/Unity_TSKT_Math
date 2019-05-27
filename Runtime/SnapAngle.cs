using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public static class SnapAngle
    {
        public static Vector2 SnapTo8(float horizontal, float vertical)
        {
            var snappedAngle = GetSnappedAngle(horizontal, vertical, 45f * Mathf.Deg2Rad);
            var magnitude = Mathf.Sqrt(horizontal * horizontal + vertical * vertical);
            return new Vector2(
                Mathf.Cos(snappedAngle) * magnitude,
                 Mathf.Sin(snappedAngle) * magnitude);
        }

        public static Vector2 SnapTo16(float horizontal, float vertical)
        {
            var snappedAngle = GetSnappedAngle(horizontal, vertical, 22.5f * Mathf.Deg2Rad);
            var magnitude = Mathf.Sqrt(horizontal * horizontal + vertical * vertical);
            return new Vector2(
                Mathf.Cos(snappedAngle) * magnitude,
                Mathf.Sin(snappedAngle) * magnitude);
        }

        static float GetSnappedAngle(float x, float y, float snapAngle)
        {
            var angle = Mathf.Atan2(y, x);

            var index = Mathf.RoundToInt(angle / snapAngle);
            return index * snapAngle;
        }
    }
}
