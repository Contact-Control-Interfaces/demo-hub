using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class TripleClick : VibrationEffect
    {
        public TripleClick()
            : base(EffectType.TripleClick, EffectStrength._100) { }

        internal override Func<VibrationEffect> Factory => () => new TripleClick();
        internal override Func<EffectOptions> OptionsFactory => null;
    }
}
