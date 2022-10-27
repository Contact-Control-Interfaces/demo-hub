using Maestro;
using Maestro.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollButton : MonoBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    public bool isDown = false;

    public float scrollSpeed = 100f;

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
        CurrentlyColliding++;
        isDown = true;
    }

    public void Deregister(FingerCollider fc)
    {
        CurrentlyColliding--;

        if (CurrentlyColliding <= 0)
        {
            isDown = false;
        }
    }
}
