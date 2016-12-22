using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UI;

/*! The CustomEventData extends Unity's PointerEventData to pass along additional information.
 * Some info which we need (like texture coordinates) aren't passed to events by Unity per default.
 * This is why we added the CustomEventData, which extends the default PointerEventData.
 * All mouse events which are called (enter/exit/hover/click etc.) get a CustomEventData passed to
 * them. However, since we try to use as much of Unity's event system as possible, the data is
 * passed as a PointerEventData. To access the additional information given by CustomEventData, simply
 * cast it:
 * 		CustomEventData cEventData = eventData as CustomEventData;
 * Afterwards, make sure to check if the cast worked by checking if cEventData == null. If cEventData
 * is not null, then you can use it to access the textureCoord, delta3D etc. */
public class CustomEventData : PointerEventData {

	/*! The difference between recently hit positions in world coordinates. */
	public Vector3 delta3D;

	/*! u,v coordinates of the hit point.*/
	public Vector2 textureCoord;

	//Index of triangle hit by raycast in Mesh
	public int hitTriangleIndex;

	public ButtonType buttonType;

	public CustomEventData( EventSystem system ) : base( system ) {}
}
