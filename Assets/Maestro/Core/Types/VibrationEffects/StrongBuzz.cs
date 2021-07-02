using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class StrongBuzz : VibrationEffect
    {
        public StrongBuzz()
            : base(EffectType.StrongBuzz, EffectStrength._100) { }

        internal override Func<VibrationEffect> Factory => () => new StrongBuzz();
        internal override Func<EffectOptions> OptionsFactory => null;
    }
}
