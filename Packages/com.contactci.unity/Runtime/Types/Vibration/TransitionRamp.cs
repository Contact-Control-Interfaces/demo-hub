using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Vibration
{

#region enums
    public enum RampDirection
    {
        Up, Down
    };

    public enum RampDuration
    {
        Short, Medium, Long
    };

    public enum RampStyle
    {
        Smooth, Sharp
    };

    public enum RampRange
    {
        Half, Full
    };

    public enum RampVariant
    {
        One, Two
    };
#endregion

    public sealed class TransitionRamp : VibrationEffect
    {
        public TransitionRamp(TransitionRamp.Options options) : base(options) { }
        public TransitionRamp() : this(new TransitionRamp.Options()) { }

        internal override Func<VibrationEffect> Factory => () => new TransitionRamp();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public TransitionStrength strength;

            public override EffectStrength Strength => (EffectStrength)strength;

            public static EffectOptions DefaultOption()
            {
                return new Options(RampDuration.Long, RampStyle.Sharp, RampVariant.One, TransitionStrength._0to100);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            public RampDirection Direction {
                get {
                    bool up = Strength == EffectStrength._0to100
                           || Strength == EffectStrength._0to50;

                    return up ? RampDirection.Up : RampDirection.Down;
                }
            }

            public RampRange Range {
                get {
                    bool half = Strength == EffectStrength._0to50
                             || Strength == EffectStrength._50to0;

                    return half ? RampRange.Half : RampRange.Full;
                }
            }

            public RampDuration Duration;
            public RampStyle Style;
            public RampVariant Variant;

            public Options(RampDuration duration, RampStyle style, RampVariant variant, TransitionStrength strength)
                : base(EffectType.TransitionRamp)
            {
                Duration = duration;
                Style = style;
                Variant = variant;
                this.strength = strength;
            }

            internal Options()
                : this(RampDuration.Short, RampStyle.Sharp, RampVariant.One, TransitionStrength._0to100) { }

            /* Use Direction,Style,Range as flags to index each array */
            /*H*//*F*/
            /*SHARP*//*SMOOTH*/
            /*      UP      *//*    DOWN     */
            private byte[] ShortValues = new byte[] { 116, 92, 110, 86, 104, 80, 98, 74 };
            private byte[] MediumValues = new byte[] { 114, 90, 108, 84, 102, 78, 96, 72 };
            private byte[] LongValues = new byte[] { 112, 88, 106, 82, 100, 76, 94, 70 };

            private int Index {
                get {
                    int result = 0;
                    result += Range == RampRange.Full ? 1 : 0;
                    result += Style == RampStyle.Smooth ? 2 : 0;
                    result += Direction == RampDirection.Down ? 4 : 0;
                    return result;
                }
            }

            public override byte Value {
                get {
                    byte result = 0;

                    switch (Duration) {
                        default: throw new NotImplementedException("Duration not implemented!");
                        case RampDuration.Short: result = ShortValues[Index]; break;
                        case RampDuration.Medium: result = MediumValues[Index]; break;
                        case RampDuration.Long: result = LongValues[Index]; break;
                    }

                    /* 2nd variant is always the next value */
                    return Variant == RampVariant.One ? result : (byte)(result + 1);
                }
            }
        }
    }
}
