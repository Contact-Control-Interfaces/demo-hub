using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class FingerPaint : MonoBehaviour
    {
        private Renderer rend;
        private AudioSource source;

        public bool indexOnly;
        public bool erase;

        public float blobScale = 150f;

        public float lineWidth;

        public GameObject splotchPrefab;

        void Start()
        {
            rend = this.GetComponent<Renderer>();
            source = this.GetComponent<AudioSource>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            FingerCollider fc = collision.gameObject.GetComponent<FingerCollider>();

            if (fc != null && fc.isTip && (fc.isIndexFinger || !indexOnly)) {
                if (source) {
                    source.pitch = Random.Range(0.8f, 1.2f);
                    source.Play();
                }

                Transform paintTransform = fc.parent.transform.Find("Paint");
                GameObject paint;
                if (paintTransform == null) {
                    paint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    paint.GetComponent<SphereCollider>().enabled = false;
                    paint.GetComponent<Renderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    paint.transform.SetParent(fc.parent.transform);
                    paint.name = "Paint";
                    paint.transform.localPosition = Vector3.zero;
                    paint.AddComponent<PaintType>();
                        
                } else {
                    paint = paintTransform.gameObject;
                }
                
                //resize paint blob if necessary
                paint.transform.localScale = Vector3.one * (lineWidth / blobScale);

                PaintType pt = paint.GetComponent<PaintType>();
                Debug.Log(rend.material.color.ToString());
                pt.paintColor = rend.material.color;
                pt.splotch = splotchPrefab;
                pt.size = lineWidth;

                Renderer mr = paint.GetComponent<Renderer>();
                mr.material.color = rend.material.color;
                mr.enabled = !erase;
            }
        }
    }

    public class PaintType : MonoBehaviour
    {
        public GameObject splotch { get; set; }
        public Color paintColor { get; set; }
        public float size { get; set; }
    }
}
