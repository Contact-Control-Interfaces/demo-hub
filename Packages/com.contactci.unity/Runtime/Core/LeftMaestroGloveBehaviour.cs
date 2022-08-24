using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Maestro
{
    public class LeftMaestroGloveBehaviour : MaestroGloveBehaviour
    {
        public override IntPtr GetPointer()
        {
            return MaestroGloveConnector.Instance.GetLeftGlovePointer();
        }
    }
}
