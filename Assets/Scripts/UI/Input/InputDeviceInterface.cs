using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections;

interface InputDeviceInterface {
    RaycastHit getRaycastHit();



    void activateVisualization();
    void deactivateVisualization();
    bool isVisualizerActive();


    PointerEventData.FramePressState getLeftButtonState();

    PointerEventData.FramePressState getRightButtonState();

	PointerEventData.FramePressState getMiddleButtonState();
	Vector2 getScrollDelta();

	Vector2 getTexCoordMovement ();
	Vector3 getMovement ();

    //Creates a ray (e.g. a mouse device creates a ray from the main camera to the courser on the screen. A vive controller creates a ray from the controller in forward direction)
    Ray createRay();
}
