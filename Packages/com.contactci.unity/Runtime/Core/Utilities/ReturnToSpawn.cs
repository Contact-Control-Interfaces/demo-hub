using System.Collections;
using UnityEngine;
using System.Linq;

namespace Maestro
{
    public class ReturnToSpawn : MonoBehaviour
    {

        private Vector3 position;
        private Quaternion rotation;
        private RigidbodyConstraints constraints;

        public static GameObject smokePrefab;
        public static AudioClip disappear, appear;

        public bool destroyInstead = false;

        public Collider[] resetColliders;

        // Use this for initialization
        void Start()
        {
            if (!smokePrefab)
                smokePrefab = GameObject.Find("Smoke");

            if (!disappear)
                disappear = Resources.Load<AudioClip>("Sounds/disappear");

            if (!appear)
                appear = Resources.Load<AudioClip>("Sounds/appear");

            this.position = this.transform.position;
            this.rotation = this.transform.rotation;

            Rigidbody rb = this.GetComponent<Rigidbody>();
            if (rb)
                this.constraints = rb.constraints;


            //Physics.defaultContactOffset = 0.001f;
        }

        private void FixedUpdate()
        {
            if (this.transform.position.y < 0) {
                Poof();
            }
        }

        private IEnumerator SpawnSmoke(Vector3 location)
        {
            if (smokePrefab) {
                GameObject smoke = Instantiate(smokePrefab);
                smoke.transform.position = location;
                smoke.transform.localScale = 0.05f * Vector3.one;
                yield return new WaitForSeconds(2.0f);
                Destroy(smoke);
            }
        }

        public void Poof()
        {
            MaestroInteractable mi = this.gameObject.GetComponent<MaestroInteractable>();
            if (mi != null && mi.isGrabbed)
                return;

            // Make the poof where it disappears
            Debug.Log("Resetting " + gameObject.name);
            StartCoroutine("SpawnSmoke", this.gameObject.transform.position);
            if (disappear != null)
                AudioSource.PlayClipAtPoint(disappear, this.gameObject.transform.position, 0.10f);

            if (destroyInstead) {
                Destroy(this.gameObject);
            } else {
                // Respawn, make another poof
                StartCoroutine("SpawnSmoke", this.position);
                this.transform.SetPositionAndRotation(position, rotation);

                Rigidbody rb = this.GetComponent<Rigidbody>();
                if (rb) {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.constraints = constraints;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (resetColliders != null && resetColliders.Contains(collision.collider)) {
                Poof();
            }
        }
    }
}
