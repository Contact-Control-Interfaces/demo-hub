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

        [DllImport("MaestroAPI")]
        protected static extern bool is_glove_connected(IntPtr glovePointer);

        [DllImport("MaestroAPI")]
        protected static extern bool start_maestro_detection_service();

        public abstract IntPtr GetPointer();

        public IntPtr GlovePointer { get; set; }
        public bool Connected = false;

        public bool addHandEnableDisable = false;

        public void Start()
        {
            Connected = is_glove_connected(GetPointer());

            if (!Connected)
            {
                bool detectionRunning = start_maestro_detection_service();
                if (detectionRunning)
                    Debug.Log("Maestro detection service is running.");
                else
                    Debug.LogError("Maestro detection service is not running!");
            }
        }

        public void Update()
        {
            bool still = is_glove_connected(GetPointer());

            Connected = still;
        }
    }
}
