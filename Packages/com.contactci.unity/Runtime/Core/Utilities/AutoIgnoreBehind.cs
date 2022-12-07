using Maestro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Maestro
{
    public class AutoIgnoreBehind : MonoBehaviour
    {
        public Axis castDirection = Axis.NegY;
        public float castDistance = 0.1f;

        public Collider[] ignored;

        private Vector3 axisToDirection(Axis axis)
        {
            switch (axis) {
                default:
                case Axis.X: return this.transform.right;
                case Axis.Y: return this.transform.up;
                case Axis.Z: return this.transform.forward;
                case Axis.NegX: return -axisToDirection(Axis.X);
                case Axis.NegY: return -axisToDirection(Axis.Y);
                case Axis.NegZ: return -axisToDirection(Axis.Z);
            }
        }

        private void Awake()
        {
            Vector3 direction = axisToDirection(castDirection);

            RaycastHit[] hits = Physics.RaycastAll(
                new Ray(this.transform.position - (direction * castDistance / 2), direction),
                castDistance);

            if (hits.Length > 0) {
                List<Collider> _ignored = new List<Collider>();

                Collider[] allColliders = this.GetComponentsInChildren<Collider>();

                Debug.Log($"Found {hits.Length} colliders to ignore for object {this.gameObject.name}");

                foreach (RaycastHit hit in hits) {
                    if (Array.IndexOf(allColliders, hit.collider) < 0) {
                        _ignored.Add(hit.collider);

                        foreach (Collider c in allColliders) {
                            Physics.IgnoreCollision(hit.collider, c);
                        }
                    }
                }

                ignored = _ignored.ToArray();
            }
        }
    }
}
