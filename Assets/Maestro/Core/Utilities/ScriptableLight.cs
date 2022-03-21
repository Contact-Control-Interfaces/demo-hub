using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public class ScriptableLight : MonoBehaviour
    {
        public new Light light;

        public virtual void SetLight(bool isOn)
        {
            light.gameObject.SetActive(isOn);
            light.enabled = isOn;
        }
    }
}
