using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class TransitionHum : VibrationEffect
    {
        public TransitionHum(TransitionHum.Options options) : base(options) { }
        public TransitionHum() : this(new TransitionHum.Options()) { }

        internal override Func<VibrationEffect> Factory => () => new TransitionHum();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public SixStrengthOptions strength;

            public override EffectStrength Strength => (EffectStrength)strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(SixStrengthOptions._100);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            public override byte Value {
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException(string.Format("This effect doesn't have strength {0}!", Strength));
                        case EffectStrength._100: return 64;
                        case EffectStrength._80: return 65;
                        case EffectStrength._60: return 66;
                        case EffectStrength._40: return 67;
                        case EffectStrength._20: return 68;
                        case EffectStrength._10: return 69;
                    }
                }
            }

            public Options(SixStrengthOptions strength)
                : base(EffectType.TransitionHum)
            {
                this.strength = strength;
            }

            internal Options() : this(SixStrengthOptions._100) { }
        }
    }
}
