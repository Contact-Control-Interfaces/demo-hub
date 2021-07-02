using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class None : VibrationEffect
    {
        public None()
            :base(EffectType.None) { }

        protected override bool DisplayStrength => false;

        internal override Func<VibrationEffect> Factory => () => new None();
        internal override Func<EffectOptions> OptionsFactory => null;
    }
}
