using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class BallCannonTarget : MonoBehaviour
    {
        public BallCannon from;
        public static float fadeOut = 0.25f;

        public float radius;
        public float line_length;

        public Transform center;
        public Transform[] toOffset;

        public bool collapse;
        [Range(0.0f, 1.0f)]
        public float ratio = 1f;

        private List<Vector3> history;
        private List<float> times;
        private static int historyLength = 50;

        private float age;
        private static float wait = 1f;

        private float AverageSpeed {
            get {
                if (history.Count <= 1)
                    return 0;

                float result = 0;
                for (int i = 1; i < history.Count; i++) {
                    result += (history[i] - history[i - 1]).magnitude / (times[i] - times[i - 1]);
                }
                return result / (history.Count - 1);
            }
        }

        void Start()
        {
            age = 0f;
            history = new List<Vector3>();
            times = new List<float>();
            RecordHistory(this.transform.position, Time.time);
        }

        void Update()
        {
            age += Time.deltaTime;

            RecordHistory(this.transform.position, Time.time);

            // collapse/slow down crosshair, fire once stopped
            bool moving = AverageSpeed > from.speedThreshold;
            if (collapse && age > wait) {
                if (moving) {
                    ratio = 1f;
                } else {
                    ratio -= Time.deltaTime;
                    if (ratio < 0) {
                        collapse = false;
                        ratio = 0;
                        from.Launch();
                        StartCoroutine(DestroySelf(fadeOut));
                    }
                }
            }

            // rotate 45deg in all dimensions per second
            this.transform.Rotate(Time.deltaTime * 45f * ratio * Vector3.one);

            // size/position the lines
            foreach (Transform t in toOffset) {
                t.localScale = new Vector3(t.localScale.x, line_length, t.localScale.z);
                t.position = this.transform.position + (t.up * ((ratio * radius) + line_length + center.localScale.y / 2));
            }
        }

        private IEnumerator DestroySelf(float delay)
        {
            yield return new WaitForSeconds(delay);
            this.gameObject.SetActive(false);
            Destroy(this.gameObject);
        }

        private void RecordHistory(Vector3 toAdd, float when)
        {
            history.Add(toAdd);
            times.Add(when);
            while (history.Count > historyLength)
                history.RemoveAt(0);
            while (times.Count > historyLength)
                times.RemoveAt(0);
        }
    }
}
