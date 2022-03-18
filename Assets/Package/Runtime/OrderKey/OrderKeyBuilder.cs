using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT
{
    public class OrderKeyBuilder
    {
        OrderKeyCombine combine = new OrderKeyCombine();
        readonly List<ulong> keys = new List<ulong>();

        public void AppendUint64(ulong key)
        {
            if (combine.Append(key, out var filledKey))
            {
                keys.Add(filledKey);
            }
        }

        public void AppendUint32(uint key)
        {
            if (combine.Append(key, out var filledKey))
            {
                keys.Add(filledKey);
            }
        }

        public void AppendUint16(ushort key)
        {
            if (combine.Append(key, out var filledKey))
            {
                keys.Add(filledKey);
            }
        }

        public void AppendUint8(byte key)
        {
            if (combine.Append(key, out var filledKey))
            {
                keys.Add(filledKey);
            }
        }

        public void AppendBool(bool key)
        {
            if (combine.Append(key, out var filledKey))
            {
                keys.Add(filledKey);
            }
        }

        public OrderKey3 ToOrderKey3()
        {
            if (keys.Count > 3)
            {
                throw new System.Exception();
            }
            if (keys.Count == 3 && combine.Length > 0)
            {
                throw new System.Exception();
            }
            return new OrderKey3(
                keys.Count > 0 ? keys[0] : keys.Count == 0 ? combine.Result : 0,
                keys.Count > 1 ? keys[1] : keys.Count == 1 ? combine.Result : 0,
                keys.Count > 2 ? keys[2] : keys.Count == 2 ? combine.Result : 0);
        }
    }
}
