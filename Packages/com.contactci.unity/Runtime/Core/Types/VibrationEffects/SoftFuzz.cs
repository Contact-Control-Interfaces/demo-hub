using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class SoftFuzz : VibrationEffect
    {
        public SoftFuzz()
            : base(EffectType.SoftFuzz, EffectStrength._60) { }

        internal override Func<VibrationEffect> Factory => () => new SoftFuzz();
        internal override Func<EffectOptions> OptionsFactory => null;
    }
}
