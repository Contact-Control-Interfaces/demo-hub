using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class BallCannon : MonoBehaviour
    {
        public GameObject toSpawn;
        public Transform barrelTransform;

        public Transform aimAtRight;
        //public Transform aimAtLeft;

        public float launchSpeed;

        //public bool targetLeft = false;
        public TextMesh targetText;

        public float persistMin = 0.1f, persistMax = 0.25f;

        public Transform endOfBarrel;

        private AudioSource source;
        private GameObject displayBall;
        private GameObject smokePrefab;

        void Start()
        {
            displayBall = Instantiate(toSpawn);

            Destroy(displayBall.GetComponent<Collider>());
            Destroy(displayBall.GetComponent<Rigidbody>());

            displayBall.transform.position = barrelTransform.position;
            displayBall.transform.parent = barrelTransform;

            source = this.GetComponent<AudioSource>();

            if (!smokePrefab)
                smokePrefab = GameObject.Find("Smoke");
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

        void Update()
        {
            if (aimAtRight != null) {
                Vector3 target = aimAtRight.position;

                Vector3 toTarget = target - barrelTransform.position;

                float s2 = launchSpeed * launchSpeed;
                float s4 = s2 * s2;

                Vector3 groundToTarget = new Vector3(toTarget.x, 0, toTarget.z);
                float x = groundToTarget.magnitude;

                float y = toTarget.y;
                float gravity = Physics.gravity.y;
                if (gravity < 0) gravity *= -1;

                // quadratic formula
                float sqrt = Mathf.Sqrt(s4 - gravity * ((gravity * x * x) + (2 * s2 * y)));
                float low = Mathf.Atan2(s2 - sqrt, gravity * x);
                float high = Mathf.Atan2(s2 + sqrt, gravity * x);

                Vector3 launchDirection = GetLaunchDirection(groundToTarget, low);

                barrelTransform.transform.rotation = Quaternion.LookRotation(launchDirection);
            }
        }

        /*public void SetTargettingLeft(bool targetLeft)
        {
            this.targetLeft = targetLeft;

            if (targetText != null) {
                targetText.text = string.Format("Target:\n{0}", this.targetLeft ? "LEFT" : "RIGHT");
            }
        }*/

        private Vector3 GetLaunchDirection(Vector3 ground, float angle)
        {
            Vector3 upComponent = Vector3.up * Mathf.Sin(angle) * launchSpeed;
            Vector3 groundComponent = ground.normalized * Mathf.Cos(angle) * launchSpeed;
            return upComponent + groundComponent;
        }

        public void Launch()
        {
            Launch(barrelTransform.transform.forward * launchSpeed);
        }

        void Launch(Vector3 launchVelocity)
        {
            GameObject spawned = Instantiate(toSpawn);
            spawned.transform.position = barrelTransform.position;

            Rigidbody rb = spawned.GetComponent<Rigidbody>();
            if (!rb) rb = spawned.AddComponent<Rigidbody>();

            MaestroInteractable interactable = spawned.GetComponent<MaestroInteractable>();
            interactable.isPersistent = true;
            interactable.persistenceDuration = Mathf.Clamp((launchSpeed / 15.0f) * persistMax, persistMin, persistMax);

            rb.constraints = RigidbodyConstraints.None;
            rb.velocity = launchVelocity;

            if (source != null) {
                source.Play();
            }

            if (smokePrefab != null) {
                StartCoroutine("SpawnSmoke", endOfBarrel.position);
            }
        }

        public void SetSpeed(float speed)
        {
            launchSpeed = speed;
        }
    }
}
