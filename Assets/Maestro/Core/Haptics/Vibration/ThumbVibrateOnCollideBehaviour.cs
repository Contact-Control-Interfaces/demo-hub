using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Maestro.Haptics.Vibration
{
    public class ThumbVibrateOnCollideBehaviour : VibrateOnCollideBehaviour
    {
        [DllImport("MaestroAPI")]
        public static extern void set_thumb_vibration_effect(IntPtr maestroPtr, byte effect);

        protected override VibrationMutator getVibrationMutator()
        {
            return set_thumb_vibration_effect;
        }
    }
}
