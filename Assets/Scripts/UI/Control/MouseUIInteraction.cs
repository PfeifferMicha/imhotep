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
		pointer.position = pos;//UICamera.WorldToScreenPoint(pos);//Camera.main.transform.position + mouseElement.transform.position - Camera.main.transform.position);
		//Ray ray = UICamera.ScreenPointToRay( pos );
		//Debug.DrawRay (ray.origin, ray.direction, Color.green);

		//Vector3 position3D = UICamera.ScreenToWorldPoint ( new Vector3( pos.x, pos.y, UICamera.nearClipPlane + 1 ) );
		//pointer.position = UICamera.WorldToScreenPoint (position3D);
		/*Debug.Log ("Position: " + position3D);
		//GameObject.Find ("Mouse3D").transform.position = p;
		GameObject mouse = GameObject.Find("Mouse3D");
		mouse.transform.position = position3D;*/


        // shoot ray
        EventSystem.current.RaycastAll(pointer, raycastResults);

		// Debug.Log ("Objects under mouse: " + raycastResults.Count);

        // handle hits
        /*foreach (RaycastResult rr in raycastResults)
        {
            // hit contains a dragable ui element
			if (Input.GetMouseButton(0) && rr.gameObject.GetComponent<DragableUI>() != null )
            {
                rr.gameObject.GetComponent<DragableUI>().MoveElement();
            }

            // hit contains a button component
			if (rr.gameObject.GetComponent<Selectable> () != null) {
				Selectable b = rr.gameObject.GetComponent<Selectable> ();

				//HOVER
				if (!hoverList.Contains (b)) {
					//PointerEventData p = new PointerEventData(EventSystem.current);
					b.OnPointerEnter (p);
					hoverList.Add (b);
					//Debug.Log("Added button, count: " + hoverList.Count);
				}

				//BUTTON DOWN
				if (Input.GetMouseButtonDown (0)) {
					//PointerEventData p = new PointerEventData(EventSystem.current);
					b.OnPointerDown (p);
				}

				//BUTTON UP
				if (Input.GetMouseButtonUp (0)) {
					//PointerEventData p = new PointerEventData(EventSystem.current);
					b.OnPointerUp (p);
					//b.OnSubmit(p); //TODO works also but what is the difference?
					if (b is Button) {
						Button button = (Button)b;
						button.OnPointerClick (p);
					} else if (b is Dropdown) {
						Dropdown dropdown = (Dropdown)b;
						dropdown.OnPointerClick (p);
					}
				}
			} else if (rr.gameObject.GetComponent<Toggle> () != null) {
				Toggle b = rr.gameObject.GetComponent<Toggle> ();

				//BUTTON DOWN
				if (Input.GetMouseButtonDown (0)) {
					//PointerEventData p = new PointerEventData(EventSystem.current);
					b.OnPointerDown (p);
				}

				//BUTTON UP
				if (Input.GetMouseButtonUp (0)) {
					//PointerEventData p = new PointerEventData(EventSystem.current);
					b.OnPointerUp (p);
					//b.OnSubmit(p); //TODO works also but what is the difference?
				}
			}

        }

        //TODO geht besser
        //un-hover all buttons not in raycastResults
		List<Selectable> deleteList = new List<Selectable>();
		foreach (Selectable b in hoverList)
        {
            bool isInRR = false;
            foreach (RaycastResult rr in raycastResults)
            {
				if(rr.gameObject.GetComponent<Selectable>() != null && rr.gameObject.GetComponent<Selectable>() == b)
                {
                    isInRR = true;
					break;
                }
            }

            if (!isInRR)
            {
                deleteList.Add(b);
            }
        }
		foreach(Selectable b in deleteList)
        {
            //PointerEventData p = new PointerEventData(EventSystem.current);
            if (b != null)
            {
                b.OnPointerExit(p);
                hoverList.Remove(b);
            }
            //Debug.Log("Removed button, count: " + hoverList.Count);
        }*/


    }


}
