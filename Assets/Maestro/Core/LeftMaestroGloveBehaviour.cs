using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Maestro
{
    public class LeftMaestroGloveBehaviour : MaestroGloveBehaviour
    {
        [DllImport("MaestroAPI")]
        public static extern IntPtr get_left_glove_pointer();

        public override IntPtr GetPointer()
        {
            return get_left_glove_pointer();
        }
    }
}
