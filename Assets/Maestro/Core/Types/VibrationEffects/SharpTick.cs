using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class SharpTick : VibrationEffect
    {
        internal SharpTick(SharpTick.Options options) : base(options) { }

        public SharpTick(NarrowThreeOptions strength) : this(new SharpTick.Options(strength)) { }

        internal SharpTick() : this(NarrowThreeOptions._100) { }

        internal override Func<VibrationEffect> Factory => () => new SharpTick();
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
                        default: throw new NotImplementedException("Strength not implemented!");
                        case EffectStrength._100: return 24;
                        case EffectStrength._80: return 25;
                        case EffectStrength._60: return 26;
                    }
                }
            }

            public Options(NarrowThreeOptions strength)
                : base(EffectType.SharpTick) 
            {
                this.strength = strength;
            }
        }
    }
}
