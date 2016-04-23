using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ShootCameraRay : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Y) )
        {
            RaycastHit hit;
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            LayerMask onlyMeshViewLayer = 1000000000; // hit only the mesh view layer

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyMeshViewLayer))
            {
                //Debug.Log("Hidden object: " + hit.collider.gameObject);
                Vector3 offset = new Vector3(0.1f, 0.1f, 0.1f);
                Debug.DrawLine(Camera.main.transform.position + offset, hit.point, Color.red, 10f);
            }
        }
    }
}
