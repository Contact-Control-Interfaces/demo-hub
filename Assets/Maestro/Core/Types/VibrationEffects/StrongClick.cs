using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class StrongClick : VibrationEffect
    {
        internal StrongClick(Options options) : base(options) { }

        public StrongClick(FourOptions strength) : this(new Options(strength)) { }

        internal StrongClick() : this(FourOptions._100) { }

        internal override Func<VibrationEffect> Factory => () => new StrongClick();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public FourOptions strength;

            public override EffectStrength Strength => (EffectStrength)strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(FourOptions._100);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            public override byte Value {
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException(string.Format("This effect doesn't have strength {0}!", Strength));
                        case EffectStrength._100: return 17;
                        case EffectStrength._80: return 18;
                        case EffectStrength._60: return 19;
                        case EffectStrength._30: return 20;
                    }
                }
            }

            public Options(FourOptions strength)
                : base(EffectType.StrongClick) 
            {
                this.strength = strength;
            }
        }
    }
}
