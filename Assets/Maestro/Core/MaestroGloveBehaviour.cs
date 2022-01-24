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
        public enum HapticCollisionModes
        {
            ON_ALL,
            ON_TAG_MATCH,
            ON_TAG_DIFFERENT
        };

        public abstract IntPtr GetPointer();

        public IntPtr GlovePointer { get; set; }
        public bool Connected = false;

        public bool addHandEnableDisable = false;

        public void Start()
        {
            Connected = MaestroGloveConnector.isGloveConnected(GetPointer());

            if (!Connected)
            {
                bool detectionRunning = MaestroGloveConnector.StartScanningForGloves();
                if (detectionRunning)
                    Debug.Log("Maestro detection service is running.");
                else
                    Debug.LogError("Maestro detection service is not running!");
            }
        }

        public void Update()
        {
            bool still = MaestroGloveConnector.isGloveConnected(GetPointer());

            Connected = still;
        }
    }
}
