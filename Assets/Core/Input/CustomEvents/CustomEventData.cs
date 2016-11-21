using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UI;

public class CustomEventData : PointerEventData {

	public Vector3 delta3D;

	public Vector2 textureCoord;

	public ButtonType buttonType;

	public CustomEventData( EventSystem system ) : base( system ) {}
}
