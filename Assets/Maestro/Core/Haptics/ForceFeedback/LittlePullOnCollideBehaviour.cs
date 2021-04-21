using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Maestro.Haptics.ForceFeedback
{
    public class LittlePullOnCollideBehaviour : PullOnCollideBehaviour
    {
        [DllImport("MaestroAPI")]
        public static extern void set_little_motor_amplitude(IntPtr maestroPtr, byte amplitude);

        protected override ForceFeedbackMutator getForceFeedbackMutator()
        {
            return set_little_motor_amplitude;
        }
    }
}
