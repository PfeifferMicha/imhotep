using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UI;

public class ClickNotifier : MonoBehaviour, IPointerClickHandler, IPointerHoverHandler {

	public delegate void NotificationEvent( PointerEventData eventData );

	public NotificationEvent clickNotificationEvent;
	public NotificationEvent hoverNotificationEvent;


	public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData )
	{
		
		if (clickNotificationEvent != null) {
			clickNotificationEvent (eventData);
		}
	}

	public void OnPointerHover (UnityEngine.EventSystems.PointerEventData eventData) {
		Debug.Log ("Hover");
		if (hoverNotificationEvent != null) {
			hoverNotificationEvent (eventData);
		}
	}

}
