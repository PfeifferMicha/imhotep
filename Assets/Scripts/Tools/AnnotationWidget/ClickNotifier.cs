using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ClickNotifier : MonoBehaviour, IPointerClickHandler {

	public delegate void NotificationEvent( PointerEventData eventData );

	public NotificationEvent notificationEvent;

	public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData )
	{
		if (notificationEvent != null) {
			notificationEvent (eventData);
		}
	}
}
