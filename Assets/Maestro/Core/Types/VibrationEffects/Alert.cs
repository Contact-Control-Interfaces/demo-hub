using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Maestro.Vibration
{
    public enum AlertDurations 
    {
        [InspectorName("750 ms")]
        ms750,
        [InspectorName("1000 ms")] 
        ms1000
    };

    public sealed class Alert : VibrationEffect
    {
        private static string DurationToString(AlertDurations duration)
        {
            switch (duration) {
                case AlertDurations.ms750: return "750 ms";
                default:
                case AlertDurations.ms1000: return "1000 ms";
            }
        }

        protected override bool DisplayStrength => true;

        internal Alert(Alert.Options options) : base(options) { }

        public Alert(AlertDurations duration) : this(new Alert.Options(duration)) { }

        internal Alert() : this(new Alert.Options(AlertDurations.ms1000)) { }

        internal override Func<VibrationEffect> Factory => () => new Alert();
        internal override Func<EffectOptions> OptionsFactory => () => Options.DefaultOption();

        [Serializable]
        public sealed class Options : EffectOptions
        {
            public AlertDurations Duration;

            public override EffectStrength Strength => EffectStrength._100;

            public static EffectOptions DefaultOption()
            {
                return new Options(AlertDurations.ms1000);
            }

            public override EffectOptions NewInstance()
            {
                return DefaultOption();
            }

            public Options(AlertDurations duration)
                : base(EffectType.Alert)
            {
                Duration = duration;
            }

            public override byte Value {
                get {
                    return (byte)(Duration == AlertDurations.ms750 ? 15 : 16);
                }
            }
        }
    }
}
