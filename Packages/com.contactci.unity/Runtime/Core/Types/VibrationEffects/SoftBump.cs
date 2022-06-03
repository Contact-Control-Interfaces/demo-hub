using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class SoftBump : VibrationEffect
    {
        internal SoftBump(Options options) : base(options) { }

        public SoftBump(WideThreeOptions strength) : this(new Options(strength)) { }

        internal SoftBump() : this(WideThreeOptions._100) { }

        internal override Func<VibrationEffect> Factory => () => new SoftBump();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public WideThreeOptions strength;

            public override EffectStrength Strength => (EffectStrength)strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(WideThreeOptions._100);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            public Options(WideThreeOptions strength)
                : base(EffectType.SoftBump) 
            {
                this.strength = strength;
            }

            public override byte Value {
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException(string.Format("This effect doesn't have strength {0}!", Strength));
                        case EffectStrength._100: return 7;
                        case EffectStrength._60: return 8;
                        case EffectStrength._30: return 9;
                    }
                }
            }
        }
    }
}
