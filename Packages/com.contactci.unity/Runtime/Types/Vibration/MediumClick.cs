using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class MediumClick : VibrationEffect
    {
        internal MediumClick() : this(NarrowThreeOptions._100) { }

        internal MediumClick(MediumClick.Options options) : base(options) { }

        public MediumClick(NarrowThreeOptions strength) : this(new MediumClick.Options(strength)) { }

        internal override Func<VibrationEffect> Factory => () => new MediumClick();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public NarrowThreeOptions strength;

            public override EffectStrength Strength => (EffectStrength)strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(NarrowThreeOptions._100);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            public override byte Value {
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException(string.Format("This effect doesn't have strength {0}!", Strength));
                        case EffectStrength._100: return 21;
                        case EffectStrength._80: return 22;
                        case EffectStrength._60: return 23;
                    }
                }
            }

            public Options(NarrowThreeOptions strength)
                 : base(EffectType.MediumClick) 
            {
                this.strength = strength;
            }
        }
    }
}
