using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoItem : MonoBehaviour
{

    [SerializeField]
    private Image childImage;

    public void ChangeImage(Sprite image)
    {
        childImage.sprite = image;
    }
}
