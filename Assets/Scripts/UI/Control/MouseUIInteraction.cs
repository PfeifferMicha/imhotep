using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MouseUIInteraction : MonoBehaviour
{
//	private List<Selectable> hoverList;

//    private PointerEventData p = new PointerEventData(EventSystem.current);

	private Camera UICamera;
	private Mouse3DMovement mMouse;
	private Vector2 mTextureSize;

    // Use this for initialization
    void Start()
    {
		//hoverList = new List<Selectable>();
		mMouse = GameObject.Find ("Mouse3D").GetComponent<Mouse3DMovement> ();
		UICamera = GameObject.Find ("UICamera").GetComponent<Camera>();
		mTextureSize.x = UICamera.targetTexture.width;
		mTextureSize.y = UICamera.targetTexture.height;
    }

    // Update is called once per frame
    void Update()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        List<RaycastResult> raycastResults = new List<RaycastResult>();

        // convert to a 2D position
		Vector2 pos = mMouse.getUVCoordinates();
		pos.x *= mTextureSize.x;
		pos.y *= mTextureSize.y;
		pointer.position = pos;

        // shoot ray
        EventSystem.current.RaycastAll(pointer, raycastResults);



    }


}
