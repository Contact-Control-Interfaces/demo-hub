using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

namespace Maestro.Haptics.ForceFeedback
{
    public abstract class PullOnCollideBehaviour : MonoBehaviour
    {
        public const byte NoHapticsEffect = 0;

        public delegate void ForceFeedbackMutator(IntPtr maestroPtr, byte amplitude);

        private MaestroGloveBehaviour ParentGloveBehavior;

        /*[Range(0, 255)]
        public byte Amplitude = 200;*/

        [Range(0, 255)]
        public byte Default = 200;

        [Range(0, 255)]
        public byte Idle = 0;

        [Range(0, 255)]
        public byte? Amplitude;

        public bool Pulling = false;
        //private MaestroInteractable interactable = null;
        private bool LastPulling = false;

        public byte? PaintAmp = null;

        public bool DribbleOverride = false;
        public float DribbleWait = 0f;
        //public static float MinDribbleWait = 1f; //moved to DribbleLengthener to change in real time

        [Header("Haptic Mode")]
        public string Tag = "Haptics";
        public MaestroGloveBehaviour.HapticCollisionModes collisionMode;

        void Start()
        {
            ParentGloveBehavior = GetComponentInParent<MaestroGloveBehaviour>();
        }

        public void Prop()
        {
            if (ParentGloveBehavior)
            {
                getForceFeedbackMutator()(ParentGloveBehavior.GetPointer(), Amplitude ?? (PaintAmp ?? Idle));

                /*
                //Just started
                if (Pulling && !LastPulling)
                {
                    getForceFeedbackMutator()(ParentGloveBehavior.GetPointer(), interactable == null ? Amplitude : (interactable.getMotorAmplitude() ?? Amplitude));
                }
                //Just ended
                else if (LastPulling && !Pulling)
                {
                    ResetCollisionFeedback(Default);
                }

                */
                LastPulling = Pulling;
                Pulling = false;
            }
        }

        protected abstract ForceFeedbackMutator getForceFeedbackMutator();

        private void HandleCollisionFeedback(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            if (ShouldPull(gameObject))
            {
                //MaestroInteractable interactable = gameObject.GetComponentInChildren<MaestroInteractable>();

                //getForceFeedbackMutator()(ParentGloveBehavior.GetPointer(), interactable == null ? Amplitude : interactable.getMotorAmplitude());

                Pulling = true;
            }
        }

        private void HandleCollisionFeedback(byte amplitude)
        {
            if (Pulling || gameObject == null)
                return;

            if (ShouldPull(gameObject))
            {
                //MaestroInteractable interactable = gameObject.GetComponentInChildren<MaestroInteractable>();

                //getForceFeedbackMutator()(ParentGloveBehavior.GetPointer(), interactable == null ? Amplitude : interactable.getMotorAmplitude());

                Pulling = true;
            }
        }

        private bool CompareTag(GameObject gameObject, string tag)
        {
            try
            {
                return gameObject.CompareTag(tag);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ShouldPull(GameObject collider)
        {
            if (ParentGloveBehavior == null || (ParentGloveBehavior.gameObject == null || !ParentGloveBehavior.Connected))
                return false;

            //Don't vibrate on self collisions
            if (!collider.transform.IsChildOf(ParentGloveBehavior.gameObject.transform))
            {
                switch (collisionMode)
                {
                    case MaestroGloveBehaviour.HapticCollisionModes.ON_ALL:
                        return true;
                    case MaestroGloveBehaviour.HapticCollisionModes.ON_TAG_MATCH:
                        return CompareTag(collider, Tag);
                    case MaestroGloveBehaviour.HapticCollisionModes.ON_TAG_DIFFERENT:
                        return !CompareTag(collider, Tag);
                }
            }

            return false;
        }

        private void ResetCollisionFeedback(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            ResetCollisionFeedback(Default);
        }

        private void ResetCollisionFeedback(byte amplitude)
        {
            getForceFeedbackMutator()(ParentGloveBehavior.GetPointer(), amplitude);

            Pulling = false;
        }

        void OnTriggerStay(Collider other)
        {
            HandleCollisionFeedback(other.gameObject);
        }

        void OnCollisionStay(Collision collision)
        {
            HandleCollisionFeedback(collision.gameObject);
        }
        
        void OnDestroy()
        {
            try
            {
                ResetCollisionFeedback(NoHapticsEffect);
            }
            catch (Exception) { }
        }

        /*void OnDisable()
        {
            try
            {
                ResetCollisionFeedback(NoHapticsEffect);
            }
            catch (Exception) { }
        }*/

    }
}
