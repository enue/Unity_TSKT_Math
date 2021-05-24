using UnityEngine;
#nullable enable

// Terms of Use: Easing Functions(Equations)
// Open source under the MIT License and the 3-Clause BSD License.

// MIT License
// Copyright © 2001 Robert Penner

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// BSD License
// Copyright © 2001 Robert Penner

// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
// Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace TSKT
{
    public static class EasingFunction
    {
        public static class Back
        {
            public static float EaseIn(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                float s = 1.70158f;
                return diff * (t /= duration) * t * ((s + 1) * t - s) + start;
            }

            public static float EaseIn(float start, float end, float t, float s)
            {
                const float duration = 1f;
                var diff = end - start;
                return diff * (t /= duration) * t * ((s + 1) * t - s) + start;
            }

            public static float EaseOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                float s = 1.70158f;
                return diff * ((t = t / duration - 1) * t * ((s + 1) * t + s) + 1) + start;
            }

            public static float EaseOut(float start, float end, float t, float s)
            {
                const float duration = 1f;
                var diff = end - start;
                return diff * ((t = t / duration - 1) * t * ((s + 1) * t + s) + 1) + start;
            }

            public static float EaseInOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                float s = 1.70158f;
                if ((t /= duration / 2) < 1) return diff / 2 * (t * t * (((s *= (1.525f)) + 1) * t - s)) + start;
                return diff / 2 * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2) + start;
            }

            public static float EaseInOut(float start, float end, float t, float s)
            {
                const float duration = 1f;
                var diff = end - start;
                if ((t /= duration / 2) < 1) return diff / 2 * (t * t * (((s *= (1.525f)) + 1) * t - s)) + start;
                return diff / 2 * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2) + start;
            }
        }

        public static class Bounce
        {
            public static float EaseIn(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return diff - EaseOut(start: 0, end: diff, t: duration - t) + start;
            }

            public static float EaseOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                if ((t /= duration) < (1 / 2.75f))
                {
                    return diff * (7.5625f * t * t) + start;
                }
                else if (t < (2 / 2.75f))
                {
                    return diff * (7.5625f * (t -= (1.5f / 2.75f)) * t + .75f) + start;
                }
                else if (t < (2.5 / 2.75))
                {
                    return diff * (7.5625f * (t -= (2.25f / 2.75f)) * t + .9375f) + start;
                }
                else
                {
                    return diff * (7.5625f * (t -= (2.625f / 2.75f)) * t + .984375f) + start;
                }
            }

            public static float EaseInOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                if (t < duration / 2) return EaseIn(start: 0, end: diff, t: t * 2) * .5f + start;
                else return EaseOut(start: 0, end: diff, t: t * 2 - duration) * .5f + diff * .5f + start;
            }
        }

        public static class Circ
        {
            public static float EaseIn(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return -diff * ((float)Mathf.Sqrt(1 - (t /= duration) * t) - 1) + start;
            }

            public static float EaseOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return diff * (float)Mathf.Sqrt(1 - (t = t / duration - 1) * t) + start;
            }

            public static float EaseInOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                if ((t /= duration / 2) < 1) return -diff / 2 * ((float)Mathf.Sqrt(1 - t * t) - 1) + start;
                return diff / 2 * ((float)Mathf.Sqrt(1 - (t -= 2) * t) + 1) + start;
            }
        }

        public static class Cubic
        {
            public static float EaseIn(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return diff * (t /= duration) * t * t + start;
            }

            public static float EaseOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return diff * ((t = t / duration - 1) * t * t + 1) + start;
            }

            public static float EaseInOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                if ((t /= duration / 2) < 1) return diff / 2 * t * t * t + start;
                return diff / 2 * ((t -= 2) * t * t + 2) + start;
            }
        }

        public static class Elastic
        {
            public static float EaseIn(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                if (t == 0) return start; if ((t /= duration) == 1) return end;
                float p = duration * .3f;
                float a = diff;
                float s = p / 4;
                return -(a * (float)Mathf.Pow(2, 10 * (t -= 1)) * (float)Mathf.Sin((t * duration - s) * (2 * (float)Mathf.PI) / p)) + start;
            }

            public static float EaseIn(float start, float end, float t, float a, float p)
            {
                const float duration = 1f;
                var diff = end - start;
                float s;
                if (t == 0) return start; if ((t /= duration) == 1) return end;
                if (a < Mathf.Abs(diff)) { a = diff; s = p / 4; }
                else { s = p / (2 * (float)Mathf.PI) * (float)Mathf.Asin(diff / a); }
                return -(a * (float)Mathf.Pow(2, 10 * (t -= 1)) * (float)Mathf.Sin((t * duration - s) * (2 * Mathf.PI) / p)) + start;
            }

            public static float EaseOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                if (t == 0) return start; if ((t /= duration) == 1) return end;
                float p = duration * .3f;
                float a = diff;
                float s = p / 4;
                return (a * (float)Mathf.Pow(2, -10 * t) * (float)Mathf.Sin((t * duration - s) * (2 * (float)Mathf.PI) / p) + diff + start);
            }

            public static float EaseOut(float start, float end, float t, float a, float p)
            {
                const float duration = 1f;
                var diff = end - start;
                float s;
                if (t == 0) return start; if ((t /= duration) == 1) return end;
                if (a < Mathf.Abs(diff)) { a = diff; s = p / 4; }
                else { s = p / (2 * (float)Mathf.PI) * (float)Mathf.Asin(diff / a); }
                return (a * (float)Mathf.Pow(2, -10 * t) * (float)Mathf.Sin((t * duration - s) * (2 * (float)Mathf.PI) / p) + diff + start);
            }

            public static float EaseInOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                if (t == 0) return start; if ((t /= duration / 2) == 2) return end;
                float p = duration * (.3f * 1.5f);
                float a = diff;
                float s = p / 4;
                if (t < 1) return -.5f * (a * (float)Mathf.Pow(2, 10 * (t -= 1)) * (float)Mathf.Sin((t * duration - s) * (2 * (float)Mathf.PI) / p)) + start;
                return a * (float)Mathf.Pow(2, -10 * (t -= 1)) * (float)Mathf.Sin((t * duration - s) * (2 * (float)Mathf.PI) / p) * .5f + diff + start;
            }

            public static float EaseInOut(float start, float end, float t, float a, float p)
            {
                const float duration = 1f;
                var diff = end - start;
                float s;
                if (t == 0) return start; if ((t /= duration / 2) == 2) return end;
                if (a < Mathf.Abs(diff)) { a = diff; s = p / 4; }
                else { s = p / (2 * (float)Mathf.PI) * (float)Mathf.Asin(diff / a); }
                if (t < 1) return -.5f * (a * (float)Mathf.Pow(2, 10 * (t -= 1)) * (float)Mathf.Sin((t * duration - s) * (2 * (float)Mathf.PI) / p)) + start;
                return a * (float)Mathf.Pow(2, -10 * (t -= 1)) * (float)Mathf.Sin((t * duration - s) * (2 * (float)Mathf.PI) / p) * .5f + diff + start;
            }
        }

        public static class Expo
        {
            public static float EaseIn(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return (t == 0) ? start : diff * (float)Mathf.Pow(2, 10 * (t / duration - 1)) + start;
            }

            public static float EaseOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return (t == duration) ? end : diff * (-(float)Mathf.Pow(2, -10 * t / duration) + 1) + start;
            }

            public static float EaseInOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                if (t == 0) return start;
                if (t == duration) return end;
                if ((t /= duration / 2) < 1) return diff / 2 * (float)Mathf.Pow(2, 10 * (t - 1)) + start;
                return diff / 2 * (-(float)Mathf.Pow(2, -10 * --t) + 2) + start;
            }
        }

        public static class Quad
        {
            public static float EaseIn(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return diff * (t /= duration) * t + start;
            }

            public static float EaseOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return -diff * (t /= duration) * (t - 2) + start;
            }

            public static float EaseInOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                if ((t /= duration / 2) < 1) return diff / 2 * t * t + start;
                return -diff / 2 * ((--t) * (t - 2) - 1) + start;
            }
        }

        public static class Quart
        {
            public static float EaseIn(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return diff * (t /= duration) * t * t * t + start;
            }

            public static float EaseOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return -diff * ((t = t / duration - 1) * t * t * t - 1) + start;
            }

            public static float EaseInOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                if ((t /= duration / 2) < 1) return diff / 2 * t * t * t * t + start;
                return -diff / 2 * ((t -= 2) * t * t * t - 2) + start;
            }
        }

        public static class Quint
        {
            public static float EaseIn(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return diff * (t /= duration) * t * t * t * t + start;
            }

            public static float EaseOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return diff * ((t = t / duration - 1) * t * t * t * t + 1) + start;
            }

            public static float EaseInOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                if ((t /= duration / 2) < 1) return diff / 2 * t * t * t * t * t + start;
                return diff / 2 * ((t -= 2) * t * t * t * t + 2) + start;
            }
        }

        public static class Sine
        {
            public static float EaseIn(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return -diff * (float)Mathf.Cos(t / duration * (Mathf.PI / 2)) + diff + start;
            }

            public static float EaseOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return diff * (float)Mathf.Sin(t / duration * (Mathf.PI / 2)) + start;
            }

            public static float EaseInOut(float start, float end, float t)
            {
                const float duration = 1f;
                var diff = end - start;
                return -diff / 2 * ((float)Mathf.Cos(Mathf.PI * t / duration) - 1) + start;
            }
        }

        public static float Linear(float start, float end, float t)
        {
            return Mathf.Lerp(start, end, t);
        }
    }
}
