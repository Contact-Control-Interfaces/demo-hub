using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeypadInput : MonoBehaviour {

    public TextMesh text;
    public string code;

    char[] actual;

    public UnityEvent onCodeEntry, onReset;

	// Use this for initialization
	void Start () {
        Clear();
	}
	
	// Update is called once per frame
	void Update () {
        text.text = string.Format("{0}\t{1}\t{2}\t{3}", actual[0], actual[1], actual[2], actual[3]);
	}

    public void InsertChar(string c)
    {
        for (int i = 0; i < actual.Length; i++)
        {
            if (actual[i] == '_')
            {
                actual[i] = c[0];

                CheckOk();
                break;
            }
        }
    }

    public void Reset()
    {
        this.onReset.Invoke();
        Clear();
    }

    public void Ok()
    {
        for (int i = 0; i < actual.Length; i++)
        {
            int index = actual.Length - 1 - i;
            if (actual[index] != '_')
            {
                actual[index] = '_';
                break;
            }
        }
    }

    public void CheckOk()
    {
        bool ok = true;
        for (int i = 0; i < 4; i++)
        {
            if (code[i] != actual[i])
                ok = false;
        }

        if (ok)
        {
            onCodeEntry.Invoke();
        }
    }

    public void Clear()
    {
        actual = new char[] { '_', '_', '_', '_' };
    }
}
