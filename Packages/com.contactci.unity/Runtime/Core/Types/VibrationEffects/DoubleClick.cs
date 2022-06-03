using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public enum DoubleClickType
    {
        Medium, Strong
    };

    public enum DoubleClickDuration
    {
        Short, Long
    };

    public sealed class DoubleClick : VibrationEffect
    {
        internal DoubleClick(DoubleClick.Options options) : base(options) { }

        public DoubleClick(TwoOptions strength) : this(new Options(strength)) { }

        internal DoubleClick() : this(TwoOptions._100) { }

        internal override Func<VibrationEffect> Factory => () => new DoubleClick();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public TwoOptions strength;

            public override EffectStrength Strength => (EffectStrength) strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(TwoOptions._100);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            public override byte Value {
                get {
                    switch (Strength) {
                        default: throw new NotImplementedException("Strength not implemented!");
                        case EffectStrength._100: return 10;
                        case EffectStrength._60: return 11;
                    }
                }
            }

            public Options(TwoOptions strength)
                : base(EffectType.DoubleClick) 
            {
                this.strength = strength;
            }
        }
    }
}
