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

                Transform paintTransform = collision.gameObject.transform.Find("Paint");
                GameObject paint;
                if (paintTransform == null)
                {
                    paint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    paint.GetComponent<SphereCollider>().enabled = false;
                    paint.transform.SetParent(collision.gameObject.transform);
                    paint.name = "Paint";
                    paint.transform.position = collision.gameObject.transform.position;
                    paint.transform.localScale = new Vector3(1.5f,1.5f,1.5f);
                    paint.AddComponent<PaintType>();
                }
                else
                {
                    paint = paintTransform.gameObject;
                }


                paint.GetComponent<Renderer>().material.color = rend.material.color;
                paint.GetComponent<PaintType>().paintColor = rend.material.color;
                if (erase)
                    paint.GetComponent<MeshRenderer>().enabled = false;
                paint.GetComponent<PaintType>().erase = erase;
            }
        }
    }
    public class PaintType : MonoBehaviour
    {
        public Color paintColor { get; set; }
        public bool erase { get; set; }
    }
}
