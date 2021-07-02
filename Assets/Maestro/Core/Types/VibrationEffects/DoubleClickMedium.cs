using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class DoubleClickMedium : VibrationEffect
    {
        internal DoubleClickMedium(DoubleClickMedium.Options options) : base(options) { }

        internal DoubleClickMedium() : this(DoubleClickDuration.Short, NarrowThreeOptions._100) { }

        public DoubleClickMedium(DoubleClickDuration duration, NarrowThreeOptions strength)
            : this(new DoubleClickMedium.Options(duration, strength)) { }

        internal override Func<VibrationEffect> Factory => () => new DoubleClickMedium();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public DoubleClickDuration Duration;
            public NarrowThreeOptions strength;

            public override EffectStrength Strength => (EffectStrength)strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(DoubleClickDuration.Long, NarrowThreeOptions._100);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            private bool isLong {
                get { return Duration == DoubleClickDuration.Long; }
            }

            public override byte Value {
                get { return isLong ? LongValue : ShortValue; }
            }

            private byte ShortValue { 
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException(string.Format("This effect doesn't have strength {0}!", Strength));
                        case EffectStrength._100: return 31;
                        case EffectStrength._80: return 32;
                        case EffectStrength._60: return 33;
                    }
                } 
            }

            private byte LongValue { 
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException(string.Format("This effect doesn't have strength {0}!", Strength));
                        case EffectStrength._100: return 41;
                        case EffectStrength._80: return 42;
                        case EffectStrength._60: return 43;
                    }
                } 
            }

            public Options(DoubleClickDuration duration, NarrowThreeOptions strength)
                : base(EffectType.DoubleClickMedium)
            {
                Duration = duration;
                this.strength = strength;
            }
        }
    }
}
