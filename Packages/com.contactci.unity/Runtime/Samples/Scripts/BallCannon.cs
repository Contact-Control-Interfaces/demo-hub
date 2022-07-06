using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class BallCannon : MonoBehaviour
    {
        public GameObject toSpawn;
        public Transform barrelTransform;
        public GameObject targetPrefab;

        public Transform aimAt;

        public float launchSpeed;

        public float persistMin = 0.1f, persistMax = 0.25f;

        public Transform endOfBarrel;

        private AudioSource source;
        public AudioClip launchSound;
        public AudioClip beepSound;
        public AudioClip fastBeepSound;

        private GameObject displayBall;
        private GameObject smokePrefab;
        private BallCannonTarget target;

        private bool lockOn;

        public float launchDelay = 2.0f;
        public float targetDespawnDelay = 0.25f;
        public float speedThreshold = 0.1f;

        private bool lastFastBeep;

        void Start()
        {
            displayBall = Instantiate(toSpawn);

            Destroy(displayBall.GetComponent<Collider>());
            Destroy(displayBall.GetComponent<Rigidbody>());

            source = this.GetComponent<AudioSource>();

            displayBall.transform.position = barrelTransform.position;
            displayBall.transform.parent = barrelTransform;

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

        public void RecordTouch(FingerCollider fc)
        {
            aimAt = fc.hpi.transforms.PalmCreaseMiddle;
        }

        void Update()
        {
            if (lockOn && aimAt != null) {
                Vector3 toTarget = aimAt.position - barrelTransform.position;

                bool fastBeep = target.ratio < 0.95f;
                if (fastBeep && !lastFastBeep) {
                    // switch to fast audio clip
                    source.clip = fastBeepSound;
                    source.Play();
                } else if (lastFastBeep && !fastBeep) {
                    // switch to slow audio clip
                    source.clip = beepSound;
                    source.Play();
                }

                if (target != null) {
                    target.transform.position = aimAt.position;
                }

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

                Vector3 launchDirection = GetLaunchDirection(groundToTarget, low);

                barrelTransform.transform.rotation = Quaternion.LookRotation(launchDirection);

                lastFastBeep = fastBeep;
            }
        }

        private Vector3 GetLaunchDirection(Vector3 ground, float angle)
        {
            Vector3 upComponent = Vector3.up * Mathf.Sin(angle) * launchSpeed;
            Vector3 groundComponent = ground.normalized * Mathf.Cos(angle) * launchSpeed;
            return upComponent + groundComponent;
        }

        public void LockOn()
        {
            // don't double fire
            if (!this.lockOn) {
                this.lockOn = true;

                if (source != null && beepSound != null) {
                    source.pitch = 1f;
                    source.clip = beepSound;
                    source.loop = true;
                    source.Play();
                }

                GameObject temp = Instantiate(targetPrefab);
                target = temp.GetComponent<BallCannonTarget>();
                target.from = this;
                target.gameObject.SetActive(true);
            }
        }

        public void Launch()
        {
            this.lockOn = false;
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

            if (source != null && launchSound != null) {
                if (source.isPlaying)
                    source.Stop();

                source.pitch = 1f;
                source.clip = launchSound;
                source.loop = false;
                source.Play();
            }

            if (smokePrefab != null) {
                StartCoroutine(SpawnSmoke(endOfBarrel.position));
            }
        }

        public void SetSpeed(float speed)
        {
            launchSpeed = speed;
        }
    }
}
