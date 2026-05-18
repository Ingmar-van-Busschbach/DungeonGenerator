using UnityEngine;
using UnityEngine.Events;

public class MouseClickController : MonoBehaviour
{
    public Vector3 clickPosition;
    public UnityEvent<Vector3> onClickEvent;

    void Update()
    {
        // Get the mouse click position in world space 
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hitInfo))
            {
                clickPosition = hitInfo.point;
                Debug.Log(clickPosition);

                // TODO EXERCISE 5: Trigger a Unity event to notify other scripts about the click here
                onClickEvent?.Invoke(clickPosition);
            }
        }

        DebugExtension.DebugWireSphere(clickPosition);
        Debug.DrawLine(Camera.main.transform.position, clickPosition);
    }

}
