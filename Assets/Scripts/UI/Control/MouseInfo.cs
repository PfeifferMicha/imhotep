using UnityEngine;
using System.Collections;

public class MouseInfo : MonoBehaviour {

    public enum MouseInfoStates{
        mouseOverUI,
        mouseOverMesh,
        mouseOverBackground
    }

    public MouseInfoStates mouseInfo = MouseInfoStates.mouseOverBackground;

    private Mouse3DMovement mMouse;

    // Use this for initialization
    void Start () {
        mMouse = this.gameObject.GetComponent<Mouse3DMovement>();
        if (mMouse == null)
        {
            Debug.LogError("Can't find Mouse3DMovement script");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (UI.UICore.instance.mouseIsOverUIObject)
        {
            mouseInfo = MouseInfoStates.mouseOverUI;
        }
        else
        {
            if(mMouse == null)
            {
                return;
            }

            RaycastHit hit;
            Ray ray;

            //Use different ray for mouse and vive controller
            if (mMouse.owner.name == "mouse") //TODO !!! Use interface 
            { 
                ray = new Ray(Camera.main.transform.position, mMouse.transform.position - Camera.main.transform.position); //TODO generate ray in Mouse3DMovement
            }
            else
            {
                ray = new Ray(mMouse.owner.transform.position, mMouse.owner.transform.forward);
            }


            LayerMask onlyMeshViewLayer = 1000000000; // hits only the mesh view layer
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyMeshViewLayer))
            {
                mouseInfo = MouseInfoStates.mouseOverMesh;
            }
            else
            {
                mouseInfo = MouseInfoStates.mouseOverBackground;
            }
        }
    }
}
