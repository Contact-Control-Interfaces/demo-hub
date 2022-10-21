using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicScrollView : MonoBehaviour
{
    [SerializeField]
    private Transform scrollViewContent;

    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private List<Sprite> DemoIcons;


    private void Start()
    {
        foreach (Sprite demoIcon in DemoIcons)
        {
            GameObject newDemoIcon = Instantiate(prefab, scrollViewContent);
           if (newDemoIcon.TryGetComponent<DemoItem>(out DemoItem demoItem))
            {
                demoItem.ChangeImage(demoIcon);
            }
        }
    }
}
