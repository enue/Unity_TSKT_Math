using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

namespace TSKT.Tests
{
    public class EasingFunction
    {
        [Test]
        [TestCase(0f, 1f)]
        [TestCase(-75f, 75f)]
        [TestCase(75f, -75f)]
        public void StartAndEnd(float start, float end)
        {
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Back.EaseIn(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Back.EaseIn(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Back.EaseInOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Back.EaseInOut(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Back.EaseOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Back.EaseOut(start, end, 1f)));

            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Bounce.EaseIn(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Bounce.EaseIn(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Bounce.EaseInOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Bounce.EaseInOut(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Bounce.EaseOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Bounce.EaseOut(start, end, 1f)));

            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Circ.EaseIn(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Circ.EaseIn(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Circ.EaseInOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Circ.EaseInOut(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Circ.EaseOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Circ.EaseOut(start, end, 1f)));

            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Cubic.EaseIn(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Cubic.EaseIn(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Cubic.EaseInOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Cubic.EaseInOut(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Cubic.EaseOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Cubic.EaseOut(start, end, 1f)));

            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Elastic.EaseIn(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Elastic.EaseIn(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Elastic.EaseInOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Elastic.EaseInOut(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Elastic.EaseOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Elastic.EaseOut(start, end, 1f)));

            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Expo.EaseIn(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Expo.EaseIn(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Expo.EaseInOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Expo.EaseInOut(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Expo.EaseOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Expo.EaseOut(start, end, 1f)));

            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Linear(start, end, 1f)));

            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Quad.EaseIn(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Quad.EaseIn(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Quad.EaseInOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Quad.EaseInOut(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Quad.EaseOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Quad.EaseOut(start, end, 1f)));

            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Quart.EaseIn(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Quart.EaseIn(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Quart.EaseInOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Quart.EaseInOut(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Quart.EaseOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Quart.EaseOut(start, end, 1f)));

            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Quint.EaseIn(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Quint.EaseIn(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Quint.EaseInOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Quint.EaseInOut(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Quint.EaseOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Quint.EaseOut(start, end, 1f)));

            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Sine.EaseIn(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Sine.EaseIn(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Sine.EaseInOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Sine.EaseInOut(start, end, 1f)));
            Assert.True(Mathf.Approximately(start, TSKT.EasingFunction.Sine.EaseOut(start, end, 0f)));
            Assert.True(Mathf.Approximately(end, TSKT.EasingFunction.Sine.EaseOut(start, end, 1f)));
        }
    }
}
