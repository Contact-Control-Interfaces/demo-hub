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
        public override IntPtr GetPointer()
        {
            return MaestroGloveConnector.Instance.GetRightGlovePointer();
        }
    }
}
