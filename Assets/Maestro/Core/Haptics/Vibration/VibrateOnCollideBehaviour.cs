using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Maestro.Haptics.Vibration
{
    [Serializable]
    public abstract class VibrateOnCollideBehaviour : MonoBehaviour
    {
        public const byte NoHapticsEffect = 0;

        public delegate void VibrationMutator(IntPtr maestroPtr, byte effect);

        [Range(1, 112)]
        public byte? VibrationEffect;

        public byte? PaintEffect;

        private byte _pulseEffect;
        public byte? PulseEffect {
            get
            {
                double result = _pulseEffect * Mathf.Pow(2f, -(pulseElapsed / pulseHalfLife));
                if (result > 3)
                    return (byte)result;
                else
                    return null;
            }

            set
            {
                if (value.HasValue)
                {
                    _pulseEffect = value.Value;
                    pulseElapsed = 0.0f;
                }
            }
        }
        public float pulseElapsed = 0.0f, pulseHalfLife = 0.4f;

        public string Tag = "Haptics";
        public MaestroGloveBehaviour.HapticCollisionModes collisionMode;

        public bool Vibrating, LastVibrating;

        private MaestroGloveBehaviour ParentGloveBehavior;

        void Start()
        {
            ParentGloveBehavior = GetComponentInParent<MaestroGloveBehaviour>();
        }

        public void Prop()
        {
            if (ParentGloveBehavior)
            {
                pulseElapsed += Time.fixedDeltaTime;

                if (VibrationEffect != null || PaintEffect != null || Vibrating)
                {
                    getVibrationMutator()(ParentGloveBehavior.GetPointer(), PulseEffect ?? (VibrationEffect ?? (PaintEffect ?? 1)));
                }
                else //if (LastVibrating && !Vibrating)
                {
                    getVibrationMutator()(ParentGloveBehavior.GetPointer(), NoHapticsEffect);
                }

                LastVibrating = Vibrating;
                Vibrating = false;
            }
        }

        protected abstract VibrationMutator getVibrationMutator();

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

        private bool ShouldVibrate(GameObject collider)
        {
            if (ParentGloveBehavior.gameObject == null || !ParentGloveBehavior.Connected)
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

        private void HandleCollisionFeedback(GameObject gameObject)
        {
            if (Vibrating || gameObject == null)
                return;

            if (ShouldVibrate(gameObject))
            {
                //MaestroInteractable interactable = gameObject.GetComponentInChildren<MaestroInteractable>();

                //getVibrationMutator()(ParentGloveBehavior.GetPointer(), //interactable == null ? VibrationEffect : (interactable.getVibrationEffect() ?? VibrationEffect));

                Vibrating = true;
            }
        }

        private void ResetCollisionFeedback(GameObject gameObject)
        {
            if (gameObject == null || !Vibrating)
                return;

            ResetCollisionFeedback();
        }

        private void ResetCollisionFeedback()
        {
            getVibrationMutator()(ParentGloveBehavior.GetPointer(), NoHapticsEffect);

            Vibrating = false;
        }
    
        void OnTriggerStay(Collider other)
        {
            HandleCollisionFeedback(other.gameObject);
        }

        void OnCollisionStay(Collision collision)
        {
            HandleCollisionFeedback(collision.gameObject);
        }

        /*void OnDisable()
        {
            try
            {
                ResetCollisionFeedback();
            }
            catch (Exception) { }
        }*/

        void OnDestroy()
        {
            try
            {
                ResetCollisionFeedback();
            }
            catch (Exception) { }
        }
    }
}
