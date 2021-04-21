using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Maestro.Haptics.ForceFeedback
{
    public class IndexPullOnCollideBehaviour : PullOnCollideBehaviour
    {
        [DllImport("MaestroAPI")]
        public static extern void set_index_motor_amplitude(IntPtr maestroPtr, byte amplitude);

        protected override ForceFeedbackMutator getForceFeedbackMutator()
        {
            return set_index_motor_amplitude;
        }
    }
}
