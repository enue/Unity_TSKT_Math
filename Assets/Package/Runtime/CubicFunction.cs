using System;
#nullable enable

namespace TSKT
{
    public readonly struct CubicFunction
    {
        readonly public double a;
        readonly public double b;
        readonly public double c;
        readonly public double d;

        public CubicFunction(double a, double b, double c, double d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
        readonly public double Evaluate(double x)
        {
            return a * x * x * x + b * x * x + c * x + d;
        }

        readonly public double Velocity(double x)
        {
            return 3.0 * a * x * x + 2.0 * b * x + c;
        }

        readonly public double Acceleration(double x)
        {
            return 6.0 * a * x + 2.0 * b;
        }

        // 以下のSolve系は、条件を連立一次方程式にしてガウスの消去法で解く。
        // 解が一意に定まらない場合(同じtの点を2つ渡した場合など)は係数がすべて0の関数を返す。

        static public CubicFunction Solve3PointsAndVelocity(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) p3,
            (double t, double v) v)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a * rt^3 + b * rt^2 + c * rt + d = r;
            // 3a * vt^2 + 2b * vt + c = v;
            const int n = 4;
            Span<double> m = stackalloc double[n * (n + 1)];
            SetPointRow(m, n, 0, p1);
            SetPointRow(m, n, 1, p2);
            SetPointRow(m, n, 2, p3);
            SetVelocityRow(m, n, 3, v);
            return Solve(m, n);
        }

        static public CubicFunction Solve2PointsAnd2Velocities(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) v1,
            (double t, double v) v2)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // 3a * ut^2 + 2b * ut + c = u;
            // 3a * vt^2 + 2b * vt + c = v;
            const int n = 4;
            Span<double> m = stackalloc double[n * (n + 1)];
            SetPointRow(m, n, 0, p1);
            SetPointRow(m, n, 1, p2);
            SetVelocityRow(m, n, 2, v1);
            SetVelocityRow(m, n, 3, v2);
            return Solve(m, n);
        }

        static public CubicFunction Solve2PointsAndConstantAccel(
            (double t, double v) p1,
            (double t, double v) p2,
            double accell)
        {
            // b * pt^2 + pt * c + d = p
            // b * qt^2 + qt * c + d = q
            // 2b = accell
            // a = 0
            const int n = 3;
            Span<double> m = stackalloc double[n * (n + 1)];
            SetPointRow(m, n, 0, p1);
            SetPointRow(m, n, 1, p2);
            SetAccelerationRow(m, n, 2, (0.0, accell));
            return Solve(m, n);
        }


        static public CubicFunction Solve2PointsVelocityAndAccel(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) v,
            (double t, double v) a)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // 3a * vt^2 + 2b * vt + c = v;
            // 6a * accelT + 2b = accel;
            const int n = 4;
            Span<double> m = stackalloc double[n * (n + 1)];
            SetPointRow(m, n, 0, p1);
            SetPointRow(m, n, 1, p2);
            SetVelocityRow(m, n, 2, v);
            SetAccelerationRow(m, n, 3, a);
            return Solve(m, n);
        }


        static public CubicFunction Solve3Points(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) p3)
        {
            // b * pt^2 + c * pt + d = p;
            // b * qt^2 + c * qt + d = q;
            // b * rt^2 + c * rt + d = r;
            // a = 0
            const int n = 3;
            Span<double> m = stackalloc double[n * (n + 1)];
            SetPointRow(m, n, 0, p1);
            SetPointRow(m, n, 1, p2);
            SetPointRow(m, n, 2, p3);
            return Solve(m, n);
        }

        static public CubicFunction Solve4Points(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) p3,
            (double t, double v) p4)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a * rt^3 + b * rt^2 + c * rt + d = r;
            // a * st^3 + b * st^2 + c * st + d = s;
            const int n = 4;
            Span<double> m = stackalloc double[n * (n + 1)];
            SetPointRow(m, n, 0, p1);
            SetPointRow(m, n, 1, p2);
            SetPointRow(m, n, 2, p3);
            SetPointRow(m, n, 3, p4);
            return Solve(m, n);
        }

        static public CubicFunction Solve2PointsAndVelocity(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) v)
        {
            // b * pt^2 + c * pt + d = p;
            // b * qt^2 + c * qt + d = q;
            // 2b * vt + c = v;
            // a = 0;
            const int n = 3;
            Span<double> m = stackalloc double[n * (n + 1)];
            SetPointRow(m, n, 0, p1);
            SetPointRow(m, n, 1, p2);
            SetVelocityRow(m, n, 2, v);
            return Solve(m, n);
        }

        static public CubicFunction SolvePointAndVelocityAndConstantAccel(
            (double t, double v) p,
            (double t, double v) v,
            double accel)
        {
            // b * pt^2 + c * pt + d = p;
            // 2b * vt + c = v;
            // 2b = accel
            // a = 0;
            const int n = 3;
            Span<double> m = stackalloc double[n * (n + 1)];
            SetPointRow(m, n, 0, p);
            SetVelocityRow(m, n, 1, v);
            SetAccelerationRow(m, n, 2, (0.0, accel));
            return Solve(m, n);
        }

        static public CubicFunction Solve2Points(
            (double t, double v) p1,
            (double t, double v) p2)
        {
            // c * pt + d = p;
            // c * qt + d = q;
            // a = 0;
            // b = 0;
            const int n = 2;
            Span<double> m = stackalloc double[n * (n + 1)];
            SetPointRow(m, n, 0, p1);
            SetPointRow(m, n, 1, p2);
            return Solve(m, n);
        }

        // 拡大係数行列はn行(n + 1)列。未知数は次数の高い順に並べる(n = 4ならa, b, c, d。n = 3ならb, c, d)。
        // 各行の左n列が係数、最後の列が右辺。

        /// <summary>
        /// f(t) = v となる条件の行を書き込む
        /// </summary>
        static void SetPointRow(Span<double> matrix, int n, int row, (double t, double v) point)
        {
            var offset = row * (n + 1);
            var t = point.t;
            // 次数の低い方から t^0, t^1, t^2... と埋める
            var power = 1.0;
            for (int column = n - 1; column >= 0; --column)
            {
                matrix[offset + column] = power;
                power *= t;
            }
            matrix[offset + n] = point.v;
        }

        /// <summary>
        /// f'(t) = v となる条件の行を書き込む
        /// </summary>
        static void SetVelocityRow(Span<double> matrix, int n, int row, (double t, double v) velocity)
        {
            var offset = row * (n + 1);
            var t = velocity.t;
            // x^kの微分は k * x^(k - 1)
            var power = 1.0;
            for (int column = n - 1; column >= 0; --column)
            {
                var degree = n - 1 - column;
                if (degree < 1)
                {
                    matrix[offset + column] = 0.0;
                }
                else
                {
                    matrix[offset + column] = degree * power;
                    power *= t;
                }
            }
            matrix[offset + n] = velocity.v;
        }

        /// <summary>
        /// f''(t) = v となる条件の行を書き込む
        /// </summary>
        static void SetAccelerationRow(Span<double> matrix, int n, int row, (double t, double v) acceleration)
        {
            var offset = row * (n + 1);
            var t = acceleration.t;
            // x^kの二階微分は k * (k - 1) * x^(k - 2)
            var power = 1.0;
            for (int column = n - 1; column >= 0; --column)
            {
                var degree = n - 1 - column;
                if (degree < 2)
                {
                    matrix[offset + column] = 0.0;
                }
                else
                {
                    matrix[offset + column] = degree * (degree - 1) * power;
                    power *= t;
                }
            }
            matrix[offset + n] = acceleration.v;
        }

        /// <summary>
        /// 部分ピボット選択つきのガウスの消去法で解いて、CubicFunctionにする
        /// </summary>
        static CubicFunction Solve(Span<double> matrix, int n)
        {
            var width = n + 1;

            // 前進消去
            for (int column = 0; column < n; ++column)
            {
                // 誤差を抑えるため、絶対値が最大の行をピボットにする
                var pivotRow = column;
                var pivotAbs = System.Math.Abs(matrix[column * width + column]);
                for (int row = column + 1; row < n; ++row)
                {
                    var abs = System.Math.Abs(matrix[row * width + column]);
                    if (abs > pivotAbs)
                    {
                        pivotRow = row;
                        pivotAbs = abs;
                    }
                }
                if (pivotAbs == 0.0)
                {
                    // 解が一意に定まらない
                    return default;
                }
                if (pivotRow != column)
                {
                    for (int i = column; i < width; ++i)
                    {
                        (matrix[column * width + i], matrix[pivotRow * width + i]) = (matrix[pivotRow * width + i], matrix[column * width + i]);
                    }
                }

                var pivot = matrix[column * width + column];
                for (int row = column + 1; row < n; ++row)
                {
                    var factor = matrix[row * width + column] / pivot;
                    if (factor == 0.0)
                    {
                        continue;
                    }
                    matrix[row * width + column] = 0.0;
                    for (int i = column + 1; i < width; ++i)
                    {
                        matrix[row * width + i] -= factor * matrix[column * width + i];
                    }
                }
            }

            // 後退代入。解は右辺の列に上書きする
            for (int row = n - 1; row >= 0; --row)
            {
                var sum = matrix[row * width + n];
                for (int i = row + 1; i < n; ++i)
                {
                    sum -= matrix[row * width + i] * matrix[i * width + n];
                }
                matrix[row * width + n] = sum / matrix[row * width + row];
            }

            // 未知数は次数の高い順に並んでいるので、後ろから d, c, b, a
            return new CubicFunction(
                a: n >= 4 ? matrix[(n - 4) * width + n] : 0.0,
                b: n >= 3 ? matrix[(n - 3) * width + n] : 0.0,
                c: n >= 2 ? matrix[(n - 2) * width + n] : 0.0,
                d: matrix[(n - 1) * width + n]);
        }
    }
}
