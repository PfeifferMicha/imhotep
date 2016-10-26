using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UI;

public class ClickNotifier : MonoBehaviour, IPointerClickHandler, IPointerHoverHandler, IPointerExitHandler {

	public delegate void NotificationEvent( PointerEventData eventData );

	public NotificationEvent clickNotificationEvent;
	public NotificationEvent hoverNotificationEvent;
	public NotificationEvent exitNotificationEvent;


	public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData )
	{
		
		if (clickNotificationEvent != null) {
			clickNotificationEvent (eventData);
		}
	}

	public void OnPointerHover (UnityEngine.EventSystems.PointerEventData eventData) {
		//Debug.Log ("Hover");
		if (hoverNotificationEvent != null) {
			hoverNotificationEvent (eventData);
		}
	}

	public void OnPointerExit (UnityEngine.EventSystems.PointerEventData eventData) {
		if (exitNotificationEvent != null) {
			exitNotificationEvent (eventData);
		}
	}

}
