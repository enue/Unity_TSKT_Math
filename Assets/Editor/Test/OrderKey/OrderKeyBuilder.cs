using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace TSKT.Tests
{
    public class OrderKeyBuilderTest
    {
        [Test]
        public void ToOrderKey3()
        {
            var builder = new OrderKeyBuilder();
            for (int i = 0; i < 64 * 3; ++i)
            {
                builder.AppendBool(true);
            }
            var key = builder.ToOrderKey3();
            Assert.AreEqual(ulong.MaxValue, key.primaryKey);
            Assert.AreEqual(ulong.MaxValue, key.secondaryKey);
            Assert.AreEqual(ulong.MaxValue, key.tertiaryKey);

            builder.AppendBool(true);
            Assert.Catch(() => builder.ToOrderKey3());
        }

        [Test]
        public void Append()
        {
            {
                var builder = new OrderKeyBuilder();
                builder.AppendBool(true);
                builder.AppendUint64(ulong.MaxValue);
                var key = builder.ToOrderKey3();
                Assert.AreEqual(0xffffffffffffffff, key.primaryKey);
                Assert.AreEqual(1UL << 63, key.secondaryKey);
            }

            {
                var builder = new OrderKeyBuilder();
                builder.AppendUint8(0xff);
                builder.AppendUint64(0x123456789a123456UL);
                var key = builder.ToOrderKey3();
                Assert.AreEqual(0xff123456789a1234, key.primaryKey);
                Assert.AreEqual(0x5600000000000000, key.secondaryKey);
            }

            {
                var builder = new OrderKeyBuilder();
                builder.AppendUint16(0xffff);
                builder.AppendUint64(0x123456789a123456UL);
                var key = builder.ToOrderKey3();
                Assert.AreEqual(0xffff123456789a12, key.primaryKey);
                Assert.AreEqual(0x3456000000000000, key.secondaryKey);
            }

            {
                var builder = new OrderKeyBuilder();
                builder.AppendUint32(0xffffffff);
                builder.AppendUint64(0x123456789a123456UL);
                var key = builder.ToOrderKey3();
                Assert.AreEqual(0xffffffff12345678, key.primaryKey);
                Assert.AreEqual(0x9a12345600000000, key.secondaryKey);
            }
        }
    }
}
