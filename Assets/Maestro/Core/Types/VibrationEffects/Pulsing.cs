using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public enum PulsingVariations 
    {
        Strong, Medium, Sharp
    };

    public sealed class Pulsing : VibrationEffect
    {
        internal Pulsing() : this(PulsingVariations.Sharp, TwoOptions._100) { }

        internal Pulsing(Pulsing.Options options) : base(options) { }

        public Pulsing(PulsingVariations variation, TwoOptions strength)
            : this(new Pulsing.Options(variation, strength)) { }

        internal override Func<VibrationEffect> Factory => () => new Pulsing();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public PulsingVariations Variation;
            public TwoOptions strength;

            public override EffectStrength Strength => (EffectStrength)strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(PulsingVariations.Strong, TwoOptions._100);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            public override byte Value {
                get {
                    switch (Variation) {
                        default:
                        case PulsingVariations.Strong:
                            return HighOrLow(52, 53);
                        case PulsingVariations.Medium:
                            return HighOrLow(54, 55);
                        case PulsingVariations.Sharp:
                            return HighOrLow(56, 57);
                    }
                }
            }

            private byte HighOrLow(byte high, byte low)
            {
                return (Strength == EffectStrength._100) ? high : low;
            }

            public Options(PulsingVariations variation, TwoOptions strength)
                : base(EffectType.Pulsing)
            {
                Variation = variation;
                this.strength = strength;
            }
        }
    }
}
