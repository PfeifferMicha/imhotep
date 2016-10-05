using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CustomEventData : PointerEventData {

	public Vector3 delta3D;

	public CustomEventData( EventSystem system ) : base( system ) {}
}
