using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CurvedText : MonoBehaviour
{
    public Font font;
    public Shader shader;
    public Material textMaterial;

    [Space]
    public bool Invert;

    public string Text;
    public float Radius;
    public int FontSize;
    public float characterSize = 0.01f;
    public float Spacing; //degrees
    public float startAngle;

    private string lastText;
    private float lastRadius;
    private int lastSize;
    private float lastSpacing;
    private float lastStart;
    private float lastChar;

    private List<TextMesh> letters;

    // Start is called before the first frame update
    void Start()
    {
        letters = new List<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        if (font == null || shader == null || textMaterial == null) {
            Debug.LogError("Set Font and Shader!");
        } else {
            if (Text != null && Text.Length > 0) {
                if (!Text.Equals(lastText)
                    || lastRadius != Radius
                    || lastSize != FontSize
                    || lastSpacing != Spacing
                    || lastStart != startAngle
                    || lastChar != characterSize)
                    OnChange();

                lastSize = FontSize;
                lastRadius = Radius;
                lastText = Text;
                lastSpacing = Spacing;
                lastStart = startAngle;
                lastChar = characterSize;
            } else if (letters.Count > 0) {
                Clear();
            }
        }
    }

    private void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        letters.Clear();
    }

    private void OnChange()
    {
        Clear();

        // Create TextMesh for each letter
        for (int i = 0; i < Text.Length; i++) {
            string letter = (Invert ? Text[Text.Length - i - 1] : Text[i]).ToString();

            GameObject temp = new GameObject(letter);
            temp.transform.parent = this.transform;
            temp.transform.position = Vector3.zero;

            TextMesh current = temp.AddComponent<TextMesh>();
            current.text = letter;

            Renderer renderer = current.GetComponent<Renderer>();
            renderer.sharedMaterial.shader = shader;
            renderer.material = textMaterial;

            current.font = font;
            current.alignment = TextAlignment.Center;
            current.anchor = TextAnchor.MiddleCenter;

            letters.Add(current);
        }

        UpdateSize();
        UpdatePosition();
    }



    private void UpdateSize()
    {
        foreach (TextMesh tm in letters) {
            tm.fontSize = FontSize;
            tm.characterSize = characterSize;
        }
    }

    private void UpdatePosition()
    {
        for (int i = 0; i < letters.Count; i++) {
            float angularOffset = -(i - (letters.Count / 2.0f)) * (Mathf.PI / 180) * (Spacing);
            angularOffset += startAngle * (Mathf.PI / 180);

            letters[i].transform.localPosition = new Vector3(Radius * Mathf.Cos(angularOffset), Radius * Mathf.Sin(angularOffset), 0);

            float rotation = (angularOffset * (180 / Mathf.PI)) - 90;
            if (Invert)
                rotation += 180;

            letters[i].transform.localRotation = Quaternion.Euler(0, 0, rotation);
        }
    }
}
