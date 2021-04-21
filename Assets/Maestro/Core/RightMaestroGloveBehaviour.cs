using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Maestro
{
    public class RightMaestroGloveBehaviour : MaestroGloveBehaviour
    {
        [DllImport("MaestroAPI")]
        public static extern IntPtr get_right_glove_pointer();

        public override IntPtr GetPointer()
        {
            return get_right_glove_pointer();
        }
    }
}
