using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SliderOutput))]
public class SliderOutputEditor : Editor {

    SliderOutput slider;

    void OnEnable() {
        slider = (SliderOutput)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Test Force-set slider position (0.25f)")) {
            slider.MoveLeverToPosition(0.25f);
        }
    }
}
#endif


public class SliderOutput : MonoBehaviour {

	Transform sliderTrans;
	Rigidbody RB;

	public int ID;
	public float outputNormal;
	public bool On;
	public float range;

	void Awake() {
		RB = GetComponentInChildren<Rigidbody> ();
		sliderTrans = RB.transform;
	}

	void Update() {
		sliderTrans.localPosition = new Vector3 (0, 0, Mathf.Clamp(sliderTrans.localPosition.z,0,range));
		sliderTrans.localEulerAngles = Vector3.zero;

		RB.velocity = Vector3.zero;
		RB.angularVelocity = Vector3.zero;

		outputNormal = (sliderTrans.localPosition.z / range);

		if (outputNormal > 0.75f) {
			if (!On) {
				SwitchedOn ();
			}
		} else if (outputNormal < 0.25f) {
			if (On) {
				SwitchedOff ();
			}
		}
	}

	void SwitchedOn() {
		On = true;
		//InteractiveControlManager.instance.SliderOn (ID);
	}

	void SwitchedOff() {
		On = false;
		//InteractiveControlManager.instance.SliderOff (ID);
	}

    public void MoveLeverToPosition(float normalizedPosition) {
        RB = GetComponentInChildren<Rigidbody>();
        sliderTrans = RB.transform;
        float clamped = Mathf.Clamp01(normalizedPosition);
        sliderTrans.localPosition = new Vector3(sliderTrans.localPosition.x, sliderTrans.localPosition.y, clamped * range);
        outputNormal = clamped;
    }
}
