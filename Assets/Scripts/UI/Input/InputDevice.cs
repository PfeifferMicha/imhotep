using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections;
using UI;

public interface InputDevice {

	InputDeviceManager.InputDeviceType getType ();

	Vector2 getScrollDelta();

    //Creates a ray (e.g. a mouse device creates a ray from the main camera to the courser on the screen. A vive controller creates a ray from the controller in forward direction)
    Ray createRay();

	ButtonInfo updateButtonInfo ();

	Camera getEventCamera ();

	Vector2 getTexCoordDelta();
	Vector3 get3DDelta();

	void setTexCoordDelta( Vector2 delta );
	void set3DDelta( Vector2 delta );
}
