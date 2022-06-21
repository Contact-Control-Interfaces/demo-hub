using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public enum TickDuration
    {
        Short, Long
    };

    [Serializable]
    public sealed class DoubleSharpTick : VibrationEffect
    {
        internal DoubleSharpTick(DoubleSharpTick.Options options) : base(options) { }

        public DoubleSharpTick(TickDuration duration, NarrowThreeOptions strength)
            : this(new DoubleSharpTick.Options(duration, strength)) { }

        internal DoubleSharpTick() : this(TickDuration.Short, NarrowThreeOptions._100) { }

        internal override Func<VibrationEffect> Factory => () => new DoubleSharpTick();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public TickDuration Duration;
            public NarrowThreeOptions strength;

            public override EffectStrength Strength => (EffectStrength)strength;

            public static EffectOptions DefaultOption() {
                return new Options(TickDuration.Long, NarrowThreeOptions._100);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            private bool isLong { get { return Duration == TickDuration.Long; } }

            public override byte Value { get { return isLong ? LongValue : ShortValue; } }

            private byte LongValue {
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException("Strength not implemented!");
                        case EffectStrength._100: return 44;
                        case EffectStrength._80: return 45;
                        case EffectStrength._60: return 46;
                    }
                }
            }

            private byte ShortValue {
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException("Strength not implemented!");
                        case EffectStrength._100: return 34;
                        case EffectStrength._80: return 35;
                        case EffectStrength._60: return 36;
                    }
                }
            }

            public Options(TickDuration duration, NarrowThreeOptions strength)
                : base(EffectType.DoubleSharpTick)
            {
                Duration = duration;
                this.strength = strength;
            }
        }
    }
}
