using System.Collections;
using System.Collections.Generic;
using System.IO;
using Maestro;
using Maestro.Vibration;
using UnityEditor;
using UnityEngine;

namespace Maestro
{
public class VibrationPanelBehavior : MonoBehaviour
{
    private DemoProperties[,] props = new DemoProperties[4, 5]
    {
        {
            new DemoProperties(new TripleClick(), "TripleClick", 0, 0.125f, 5.55f, 0.09f, 38.2f, 3, 0.78f),
            new DemoProperties(new SoftFuzz(), "Fuzz", 0, 0.082f,3.76f,1.11f,1f,1,0f, 0),
            new DemoProperties(new StrongClick(FourOptions._100), "SingleClick",0,0.225f,0.28f,3.08f,74.7f,1,0f),
            new DemoProperties(new SharpTick(NarrowThreeOptions._100), "SingleTick",0,0.228f,0.28f,4.36f,76.2f,1,0),
            new DemoProperties(new DoubleClickStrong(DoubleClickDuration.Short, FourOptions._100), "DoubleClick", 0, 0.125f, 5.55f, 0.09f, 38.2f, 2, 0.78f)
        },
        {
            new DemoProperties(new LongBuzz(), "Buzz",0,0.079f,10f,0.19f,48.9f,1,0.75f, 0),
            new DemoProperties(new SharpClick(WideThreeOptions._100), "SingleClick",0,0.225f,0.28f,3.08f,74.7f,1,0f),
            new DemoProperties(new Pulsing(PulsingVariations.Sharp, TwoOptions._100), "Pulse", 0,0.156f,5.55f,0.09f,87.8f,3,0.78f),
            new DemoProperties(new SmoothHum(NarrowFiveOptions._50), "Hum",0,0.064f,7.9f,2.14f,1f,1,0.59f, 0),
            new DemoProperties(new TransitionRamp(new TransitionRamp.Options(RampDuration.Long, RampStyle.Smooth,
                RampVariant.One, TransitionStrength._0to100)), "RampUp",0,0.14f,0.28f,0.96f,100f,5,0.12f)
        },
        {
            new DemoProperties(new SoftBump(WideThreeOptions._100), "Bump",0,0.125f,4.89f,0.24f,13.9f,1,0.78f),
            new DemoProperties(new TransitionClick(), "TransitionClick",0,0.171f,5.29f,0.16f,91.9f,1,0.78f),
            new DemoProperties(new Alert(AlertDurations.ms750), "Buzz",0,0.11f,8.93f,0.14f,65.1f,1,0f, 0),
            new DemoProperties(new TransitionHum(), "Hum",0,0.064f,7.9f,2.14f,1f,1,0.59f, 0),
            new DemoProperties(new DoubleSharpTick(TickDuration.Long, NarrowThreeOptions._100), "DoubleTick", 0, 0.125f, 5.55f, 0.09f, 38.2f, 2, 0.78f)
        },
        {
            new DemoProperties(new DoubleSharpTick(TickDuration.Short, NarrowThreeOptions._100), "DoubleTick", 0, 0.125f, 5.55f, 0.09f, 38.2f, 2, 0.78f),
            new DemoProperties(new Buzz(WideFiveOptions._100), "Buzz",0,0.079f,10f,0.19f,48.9f,1,0.75f, 0),
            new DemoProperties(new TransitionRamp(new TransitionRamp.Options(RampDuration.Long, RampStyle.Smooth,
                RampVariant.One, TransitionStrength._100to0)), "RampDown",0,0.202f,0.28f,0.96f,100f,1,2.65f),
            new DemoProperties(new DoubleClickMedium(DoubleClickDuration.Short, NarrowThreeOptions._100), "DoubleClick", 0, 0.125f, 5.55f, 0.14f, 38.2f, 2, 0.78f),
            new DemoProperties(new Pulsing(PulsingVariations.Medium, TwoOptions._100), "Pulse", 0,0.156f,5.55f,0.09f,87.8f,3,0.78f)
        }
    };

    public List<Color> paletteColors = new List<Color>()
    {
        Color.red,
        Color.blue,
        Color.magenta,
        Color.green
    };

    private const string TexPath = "Packages/com.contactci.unity/Runtime/Core/_Textures/VibrationHints/";

    public List<MaestroInteractable> Interactables;
    
    // Start is called before the first frame update
    void Start()
    {
        var rad = GetComponentInChildren<RadioButtonPanel>();
        if (rad != null)
        {
            for (int i = 0; i < rad.gangedButtons.Count; i++)
            {
                var bt = rad.gangedButtons[i];
                var ren = bt.GetComponentInChildren<MeshRenderer>();
                if (ren == null)
                    continue;
                if(Application.isPlaying)
                    ren.material.color = paletteColors[i];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchPalette(int index)
    {
        for (int i = 0; i < 5; i++)
        {
            var prop = props[index, i];
            var interactable = Interactables[i];
            interactable.SetHapticOverride(new HapticEffect(){Amplitude = prop.HapticStrength, Vibration = prop.Vibration});
            var plane = interactable.transform.GetChild(0).gameObject;
            var ren = plane.GetComponent<SpriteRenderer>();
            //var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(TexPath, prop.HintTexture));
            var texture = Resources.Load<Sprite>($"VibrationHints/{prop.HintTexture}");
            ren.sprite = texture;
            //ren.material.mainTexture = texture;
            var shader = interactable.gameObject.GetComponent<VibrationShaderBehavior>();
            var color = paletteColors[index];
            Color.RGBToHSV(color, out float h, out float s, out float v);
            float scalar = 0.025f;
            h += scalar * (i + Random.Range(-2f, 2f));
            var nextColor = Color.HSVToRGB(h, s, v);
            shader.Color = nextColor;
            shader.MaxAmplitude = prop.Amplitude;
            shader.Frequency = prop.Frequency;
            shader.Speed = prop.Speed;
            shader.Sharpness = prop.Sharpness;
            shader.Plurality = prop.Plurality;
            shader.PluralityPhase = prop.Phase;
        }
    }

    private struct DemoProperties
    {
        public VibrationEffect Vibration;
        public byte HapticStrength;
        public string HintTexture;

        //Shader properties
        //see VibrationShader
        public float Amplitude; //(0,1)
        public float Frequency; //(0,10)
        public float Speed;     //(0,5)
        public float Sharpness; //(1,100)
        public int Plurality;   //>=0?
        public float Phase;     //(0,6.28)
        
        //ctor
        public DemoProperties(VibrationEffect vibration, string texture, byte haptics = 0, float amplitude = -1,
            float frequency = -1, float speed = -1, float sharpness = -1, int plurality = -1, float phase = -1, int delay = 600)
        {
            Vibration = vibration;
            Vibration.RepeatDelay = delay;
            HintTexture = texture;
            HapticStrength = haptics;
            Amplitude = amplitude;
            Frequency = frequency;
            Speed = speed;
            Sharpness = sharpness;
            Plurality = plurality;
            Phase = phase;
        }
    }
}
}