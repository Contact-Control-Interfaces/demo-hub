using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class LongBuzz : VibrationEffect
    {
        internal override Func<VibrationEffect> Factory => () => new LongBuzz();
        internal override Func<EffectOptions> OptionsFactory => null;

        public LongBuzz()
            :base(EffectType.LongBuzz, EffectStrength._100) { }

        public static LongBuzz NewInstance()
        {
            return new LongBuzz();
        }
    }
}
