using UnityEngine;
using System.Collections;

/*
    If an element uses this component, you can drag this element and the parent canvas and the parent game objects moves too.
*/
public class DragableUI : MonoBehaviour {

    private bool dragUI = false;

    public float speed = 5f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (dragUI )
        {
            if (Input.GetMouseButton(0))
            {
                if (transform.parent != null && transform.parent.transform.parent != null)
                {
                    transform.parent.transform.parent.transform.localPosition = transform.parent.transform.parent.transform.localPosition + new Vector3(Input.GetAxis("Mouse X") * speed, Input.GetAxis("Mouse Y") * speed, 0);
                }
            }
            else
            {
                dragUI = false;
            }
        }
    }

    // Update is called once per frame
    public void MoveElement()
    {
        dragUI = true;
    }

}
