using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class DoubleClickStrong : VibrationEffect
    {
        internal DoubleClickStrong(DoubleClickStrong.Options options) : base(options) { }

        internal DoubleClickStrong() : this(DoubleClickDuration.Short, FourOptions._100) { }

        public DoubleClickStrong(DoubleClickDuration duration, FourOptions strength)
            : this(new DoubleClickStrong.Options(duration, strength)) { }

        internal override Func<VibrationEffect> Factory => () => new DoubleClickStrong();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            private DoubleClickDuration Duration;

            public FourOptions strength;

            public override EffectStrength Strength => (EffectStrength)strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(DoubleClickDuration.Long, FourOptions._100);
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
                        case EffectStrength._100: return 27;
                        case EffectStrength._80: return 28;
                        case EffectStrength._60: return 29;
                        case EffectStrength._30: return 30;
                    }
                } 
            }

            private byte LongValue { 
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException(string.Format("This effect doesn't have strength {0}!", Strength));
                        case EffectStrength._100: return 37;
                        case EffectStrength._80: return 38;
                        case EffectStrength._60: return 39;
                        case EffectStrength._30: return 40;
                    }
                } 
            }

            public Options(DoubleClickDuration duration, FourOptions strength)
                : base(EffectType.DoubleClickStrong) 
            {
                Duration = duration;
                this.strength = strength;
            }
        }
    }
}
