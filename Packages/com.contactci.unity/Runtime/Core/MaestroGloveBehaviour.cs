using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Maestro
{
    public abstract class MaestroGloveBehaviour : MonoBehaviour
    {
        private static bool TriedToStart = false;
        private static bool DetectionStarted = false;

        public enum HapticCollisionModes
        {
            ON_ALL,
            ON_TAG_MATCH,
            ON_TAG_DIFFERENT
        };

        public abstract IntPtr GetPointer();

        public IntPtr GlovePointer { get; set; }
        public bool Connected = false;

        public void Start()
        {
            Connected = MaestroGloveConnector.isGloveConnected(GetPointer());

            if (!Connected && !TriedToStart) {
                StartDetection();
            }
        }

        public void Update()
        {
            bool still = MaestroGloveConnector.isGloveConnected(GetPointer());

            Connected = still;
        }

        private void StartDetection()
        {
            DetectionStarted = MaestroGloveConnector.StartScanningForGloves();

            if (DetectionStarted)
                Debug.Log("Maestro detection service is running.");
            else
                Debug.LogError("Maestro detection service is not running!");

            TriedToStart = true;
        }
    }
}
