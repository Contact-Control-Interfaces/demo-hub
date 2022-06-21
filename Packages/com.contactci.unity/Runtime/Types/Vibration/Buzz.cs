using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class Buzz : VibrationEffect
    {
        public Buzz(WideFiveOptions strength)
            : this(new Buzz.Options(strength)) { }

        internal Buzz(Buzz.Options options) : base(options) { }

        internal Buzz() : this(WideFiveOptions._100) { }

        internal override Func<VibrationEffect> Factory => () => new Buzz();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public WideFiveOptions strength;

            public override EffectStrength Strength => (EffectStrength) strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(WideFiveOptions._100);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            public override byte Value {
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException("Strength not implemented!");
                        case EffectStrength._100: return 47;
                        case EffectStrength._80: return 48;
                        case EffectStrength._60: return 49;
                        case EffectStrength._40: return 50;
                        case EffectStrength._20: return 51;
                    }
                }
            }

            public Options(WideFiveOptions strength)
                : base(EffectType.Buzz) 
            {
                this.strength = strength;
            }
        }
    }
}
