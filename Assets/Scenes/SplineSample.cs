using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public class SplineSample : MonoBehaviour
    {
        [SerializeField]
        GameObject target;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
            var xPoints = new List<(double t, double v)>();
            var yPoints = new List<(double t, double v)>();
            var zPoints = new List<(double t, double v)>();

            xPoints.Add((0f, 0f));
            yPoints.Add((0f, 0f));
            zPoints.Add((0f, 0f));
            var t = 1f;

            // 区間が多いと演算誤差で変な動きになる
            for (int i = 0; i < 10; ++i)
            {
                xPoints.Add((t, Random.Range(-2f, 2f)));
                yPoints.Add((t, Random.Range(-2f, 2f)));
                zPoints.Add((t, Random.Range(-2f, 2f)));
                t += 1f;
            }
            xPoints.Add((t, 0f));
            yPoints.Add((t, 0f));
            zPoints.Add((t, 0f));

            var x = new Spline(0, 0, xPoints.ToArray());
            var y = new Spline(0, 0, yPoints.ToArray());
            var z = new Spline(0, 0, zPoints.ToArray());

            var startedTime = Time.time;
            while (true)
            {
                yield return null;
                var elapsedTime = Time.time - startedTime;
                if (elapsedTime > x.Duration)
                {
                    break;
                }
                var pos = new Vector3(
                    (float)x.Evaluate(elapsedTime),
                    (float)y.Evaluate(elapsedTime),
                    (float)z.Evaluate(elapsedTime));

                target.transform.position = pos;
            }
        }
    }
}
