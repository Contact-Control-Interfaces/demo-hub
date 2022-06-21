using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class SmoothHum : VibrationEffect
    {
        internal SmoothHum(SmoothHum.Options options)
            : base(options) { }

        public SmoothHum(NarrowFiveOptions strength)
            : this(new SmoothHum.Options(strength)) { }

        internal SmoothHum()
            : this(NarrowFiveOptions._50) { }

        internal override Func<VibrationEffect> Factory => () => new SmoothHum();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public NarrowFiveOptions strength;

            public override EffectStrength Strength => (EffectStrength)strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(NarrowFiveOptions._50);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            public override byte Value {
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException("Effect strength not implemented!");
                        case EffectStrength._50: return 119;
                        case EffectStrength._40: return 120;
                        case EffectStrength._30: return 121;
                        case EffectStrength._20: return 122;
                        case EffectStrength._10: return 123;
                    }
                }
            }

            public Options(NarrowFiveOptions strength)
                : base(EffectType.SmoothHum) 
            {
                this.strength = strength;
            }
        }
    }
}
