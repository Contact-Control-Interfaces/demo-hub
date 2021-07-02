using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{
    public sealed class SharpClick : VibrationEffect
    {
        internal SharpClick(SharpClick.Options options) : base(options) { }

        public SharpClick(WideThreeOptions strength) : this(new SharpClick.Options(strength)) { }

        internal SharpClick() : this(WideThreeOptions._100) { }

        internal override Func<VibrationEffect> Factory => () => new SharpClick();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public WideThreeOptions strength;

            public override EffectStrength Strength => (EffectStrength)strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(WideThreeOptions._100);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            public override byte Value {
                get {
                    switch (Strength) {
                        default: return 0;
                        case EffectStrength._100: return 4;
                        case EffectStrength._60: return 5;
                        case EffectStrength._30: return 6;
                    }
                }
            }

            public Options(WideThreeOptions strength)
                : base(EffectType.SharpClick) 
            {
                this.strength = strength;
            }
        }
    }
}
