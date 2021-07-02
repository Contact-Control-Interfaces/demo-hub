using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Maestro.Vibration
{
    #region Enums
    public enum EffectType
    {
        None,

        SharpTick,
        DoubleSharpTick,

        SharpClick,
        MediumClick,
        StrongClick,

        DoubleClick,
        DoubleClickStrong,
        DoubleClickMedium,

        TripleClick,

        Buzz,
        LongBuzz,
        StrongBuzz,

        Alert,
        SmoothHum,
        SoftBump,
        SoftFuzz,

        Pulsing,
        TransitionHum,
        TransitionRamp,
        TransitionClick,
    };

    public enum EffectStrength
    {
        _10, _20, _30, _40, _50, _60, _80, _100,
        _0to100, _100to0, _0to50, _50to0
    };

    public enum SixStrengthOptions
    {
        [InspectorName("10%")]
        _10 = EffectStrength._10,
        [InspectorName("20%")]
        _20 = EffectStrength._20,
        [InspectorName("40%")]
        _40 = EffectStrength._40,
        [InspectorName("60%")]
        _60 = EffectStrength._60,
        [InspectorName("80%")]
        _80 = EffectStrength._80,
        [InspectorName("100%")]
        _100 = EffectStrength._100
    };

    public enum NarrowFiveOptions
    {
        [InspectorName("10%")]
        _10 = EffectStrength._10,
        [InspectorName("20%")]
        _20 = EffectStrength._20,
        [InspectorName("30%")]
        _30 = EffectStrength._30,
        [InspectorName("40%")]
        _40 = EffectStrength._40,
        [InspectorName("50%")]
        _50 = EffectStrength._50
    };

    public enum WideFiveOptions
    {
        [InspectorName("20%")]
        _20 = EffectStrength._20,
        [InspectorName("40%")]
        _40 = EffectStrength._40,
        [InspectorName("60%")]
        _60 = EffectStrength._60,
        [InspectorName("80%")]
        _80 = EffectStrength._80,
        [InspectorName("100%")]
        _100 = EffectStrength._100
    };

    public enum FourOptions
    {
        [InspectorName("30%")]
        _30 = EffectStrength._30,
        [InspectorName("60%")]
        _60 = EffectStrength._60,
        [InspectorName("80%")]
        _80 = EffectStrength._80,
        [InspectorName("100%")]
        _100 = EffectStrength._100
    };

    public enum WideThreeOptions
    {
        [InspectorName("30%")]
        _30 = EffectStrength._30,
        [InspectorName("60%")]
        _60 = EffectStrength._60,
        [InspectorName("100%")]
        _100 = EffectStrength._100
    };

    public enum NarrowThreeOptions
    {
        [InspectorName("60%")]
        _60 = EffectStrength._60,
        [InspectorName("80%")]
        _80 = EffectStrength._80,
        [InspectorName("100%")]
        _100 = EffectStrength._100
    };

    public enum TwoOptions
    {
        [InspectorName("60%")]
        _60 = EffectStrength._60,
        [InspectorName("100%")]
        _100 = EffectStrength._100
    };

    public enum TransitionStrength
    {
        [InspectorName("0% to 50%")]
        _0to50 = EffectStrength._0to50,
        [InspectorName("0% to 100%")]
        _0to100 = EffectStrength._0to100,
        [InspectorName("50% to 0%")]
        _50to0 = EffectStrength._50to0,
        [InspectorName("100% to 0%")]
        _100to0 = EffectStrength._100to0
    };
    #endregion

    [Serializable]
    public abstract class VibrationEffect : IComparable<VibrationEffect>
    {
        private static Dictionary<EffectType, byte> _staticValues;
        private static byte GetStaticValue(EffectType type)
        {
            return _staticValues[type];
        }

        static VibrationEffect()
        {
            _staticValues = new Dictionary<EffectType, byte>();
            _staticValues.Add(EffectType.None, 0);
            _staticValues.Add(EffectType.TripleClick, 12);
            _staticValues.Add(EffectType.StrongBuzz, 14);
            _staticValues.Add(EffectType.LongBuzz, 118);
            _staticValues.Add(EffectType.SoftFuzz, 13);
        }        

        public static VibrationEffect None = new None();

        private static EffectType[] _allTypes;
        protected static EffectType[] AllTypes {
            get {
                if (_allTypes == null)
                    _allTypes = (EffectType[])Enum.GetValues(typeof(EffectType));

                return _allTypes;
            }
        }

        public override string ToString() { return Description; }

        [SerializeField]
        protected bool TakesOptions = false;

        protected virtual bool DisplayStrength => true;

        public string Description;

        public byte Value { get { return this.TakesOptions ? options.Value : GetStaticValue(this.WhichType); } }

        public EffectType WhichType;

        public EffectStrength? Strength;

        [SerializeReference]
        public EffectOptions options;

        internal abstract Func<VibrationEffect> Factory { get; }
        internal abstract Func<EffectOptions> OptionsFactory { get; }

        internal VibrationEffect(EffectType effectType)
            : this(effectType, null) { }

        internal VibrationEffect(EffectType effectType, EffectStrength? strength)
        {
            this.WhichType = effectType;
            this.Strength = strength;

            this.TakesOptions = this.OptionsFactory != null;
            if (!this.TakesOptions)
                this.options = null;
        }

        public VibrationEffect(EffectOptions options)
            : this(options.Type, options.Strength)
        {
            this.options = options;
        }

        public int CompareTo(VibrationEffect other)
        {
            if (other != null) {
                return this.Value.CompareTo(other.Value);
            }
            return -1;
        }

        public static VibrationEffect ConstructEffect(byte effectCode)
        {
            switch(effectCode) {
                default: return new None();
            }
        }

        public static VibrationEffect ConstructEffect(EffectType type)
        {
            VibrationEffect result = null;

            switch (type) {
                default: 
                    throw new NotImplementedException(string.Format("Unable to construct {0}!", type.ToString()));
                case EffectType.None: result = new None(); break;
                case EffectType.MediumClick: result = new MediumClick(); break;
                case EffectType.SharpClick: result = new SharpClick(); break;
                case EffectType.StrongClick: result = new StrongClick(); break;

                case EffectType.DoubleClick: result = new DoubleClick(); break;
                case EffectType.DoubleClickStrong: result = new DoubleClickStrong(); break;
                case EffectType.DoubleClickMedium: result = new DoubleClickMedium(); break;

                case EffectType.TripleClick: result = new TripleClick(); break;

                case EffectType.SharpTick: result = new SharpTick(); break;
                case EffectType.DoubleSharpTick: result = new DoubleSharpTick(); break;

                case EffectType.SoftFuzz: result = new SoftFuzz(); break;
                case EffectType.Alert: result = new Alert(); break;
                case EffectType.SoftBump: result = new SoftBump(); break;

                case EffectType.TransitionHum: result = new TransitionHum(); break;
                case EffectType.TransitionRamp: result = new TransitionRamp(); break;
                case EffectType.TransitionClick: result = new TransitionClick(); break;

                case EffectType.Pulsing: result = new Pulsing(); break;

                case EffectType.SmoothHum: result = new SmoothHum(); break;

                case EffectType.Buzz: result = new Buzz(); break;
                case EffectType.StrongBuzz: result = new StrongBuzz(); break;
                case EffectType.LongBuzz: result = new LongBuzz(); break;
            }

            return result;
        }

        public static string TypeToString(EffectType type)
        {
            switch (type) {
                default: return type.ToString();

                case EffectType.MediumClick: return "Medium Click";
                case EffectType.StrongClick: return "Strong Click";
                case EffectType.SharpClick: return "Sharp Click";

                case EffectType.DoubleClick: return "Double Click";
                case EffectType.DoubleClickStrong: return "Double Click Strong";
                case EffectType.DoubleClickMedium: return "Double Click Medium";

                case EffectType.TripleClick: return "Triple Click";

                case EffectType.SoftBump: return "Soft Bump";

                case EffectType.SoftFuzz: return "Soft Fuzz";
                case EffectType.StrongBuzz: return "Strong Buzz";
                case EffectType.LongBuzz: return "Long Buzz";
                case EffectType.SmoothHum: return "Smooth Hum";
                case EffectType.SharpTick: return "Sharp Tick";
                case EffectType.DoubleSharpTick: return "Double Sharp Tick";
            }
        }

        public override bool Equals(object obj)
        {
            VibrationEffect other = obj as VibrationEffect;
            if (other == null)
                return false;

            if (this.WhichType != other.WhichType)
                return false;
            else {
                if (!this.TakesOptions  /*_staticValues.ContainsKey(this.WhichType)*/)
                    return true;
                else
                    return this.options.Equals(other.options);
            }
        }
    }

    [Serializable]
    public abstract class EffectOptions
    {
        [HideInInspector]
        public EffectType Type;

        public abstract byte Value { get; }

        public abstract EffectOptions NewInstance();

        public abstract EffectStrength Strength { get; }

        public static EffectOptions ConstructFromType(EffectType type)
        {
            Func<EffectOptions> factory = VibrationEffect.ConstructEffect(type).OptionsFactory;
            if (factory != null) {
                return factory();
            } else {
                return null;
            }
        }

        internal EffectOptions(EffectType type)
        {
            this.Type = type;
        }

        public static string StrengthToString(EffectStrength strength)
        {
            switch (strength) {
                default: throw new NotImplementedException("strength not found!");
                case EffectStrength._10: return "10%";
                case EffectStrength._20: return "20%";
                case EffectStrength._30: return "30%";
                case EffectStrength._40: return "40%";
                case EffectStrength._50: return "50%";
                case EffectStrength._60: return "60%";
                case EffectStrength._80: return "80%";
                case EffectStrength._100: return "100%";

                case EffectStrength._0to50: return "0% to 50%";
                case EffectStrength._50to0: return "50% to 0%";
                case EffectStrength._0to100: return "0% to 100%";
                case EffectStrength._100to0: return "100% to 0%";
            }
        }

        public override bool Equals(object obj)
        {
            EffectOptions options = obj as EffectOptions;
            if (options == null)
                return false;

            return this.Type == options.Type && this.Value == options.Value;
        }
    }
}
