using Maestro;
using Maestro.UI;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollButton : MonoBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    public bool isDown = false;

    [SerializeField]
    public AudioSource clickAudio;


    public float scrollSpeed = 50f;

    protected int CurrentlyColliding;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
    }

    


/*    public void OnTriggerEnter(Collider collider)
    {
        numOfTouch++;
        isDown = true;
    }

    public void OnTriggerExit(Collider collider)
    {
        numOfTouch--;
        if(CurrentlyColliding == 0)
        {
            isDown = false;
        }
    }*/

    public void Register(FingerCollider fc)
    {
        Debug.Log("Pressed " + this.name);
        CurrentlyColliding++;
        isDown = true;
        //clickAudio.Play();
    }

    public void Deregister(FingerCollider fc)
    {
        Debug.Log("Let Go " + this.name);
        CurrentlyColliding--;

        if (CurrentlyColliding <= 0)
        {
            isDown = false;
        }
    }
}
