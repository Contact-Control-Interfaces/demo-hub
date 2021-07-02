using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class TransitionClick : VibrationEffect
    {
        public TransitionClick(TransitionClick.Options options) : base(options) { }
        public TransitionClick() : this(new TransitionClick.Options()) { }

        internal override Func<VibrationEffect> Factory => () => new TransitionClick();
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
                        case EffectStrength._100: return 58;
                        case EffectStrength._80: return 59;
                        case EffectStrength._60: return 60;
                        case EffectStrength._40: return 61;
                        case EffectStrength._20: return 62;
                        case EffectStrength._10: return 63;
                    }
                }
            }

            public Options(SixStrengthOptions strength)
                : base(EffectType.TransitionClick)
            {
                this.strength = strength;
            }

            internal Options() : this(SixStrengthOptions._100) { }
        }
    }
}
