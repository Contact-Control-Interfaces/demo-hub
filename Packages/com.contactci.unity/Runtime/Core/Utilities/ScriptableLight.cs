using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class ScriptableLight : MonoBehaviour
    {
        private static Material onMat;
        private static Material offMat;

        public new Light light;

        public Color color = Color.red;

        private MeshRenderer mesh;

        private void Awake()
        {
            if (onMat == null)
                onMat = Resources.Load<Material>("LED_ON");

            if (offMat == null)
                offMat = Resources.Load<Material>("LED_OFF");

            mesh = this.GetComponent<MeshRenderer>();

            light.color = color;
        }

        public virtual void SetLight(bool isOn)
        {
            light.gameObject.SetActive(isOn);
            light.enabled = isOn;

            mesh.material = isOn ? onMat : offMat;
            if (isOn)
                mesh.material.SetColor("_EmissionColor", color);
        }
    }
}
