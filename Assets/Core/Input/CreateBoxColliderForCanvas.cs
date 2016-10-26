using UnityEngine;
using System.Collections;

public class CreateBoxColliderForCanvas : MonoBehaviour {

	// Use this for initialization
	void Start () {
        RectTransform rt = this.GetComponent<RectTransform>();
        if (rt != null)
        {
            BoxCollider bc = this.gameObject.AddComponent<BoxCollider>();
            bc.center = Vector3.zero;
            bc.size = new Vector3(rt.rect.width, rt.rect.height, 0.1f);
        }
        else
        {
            Debug.LogError("No rect trnasform found");
        }

    }
}
