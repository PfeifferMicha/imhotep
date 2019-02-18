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

	public void UpdateBoxCollider() {
		RectTransform rt = this.GetComponent<RectTransform>();
		Debug.Log ("Component: " + rt.rect.width + " " +rt.rect.height);
		//try{
			BoxCollider bc = this.GetComponent<BoxCollider> ();
			Vector2 pivotCenter = new Vector2 (0.5f, 0.5f);
			if (rt.pivot != pivotCenter) {
				if (rt.pivot.x==1){
					bc.center = new Vector3 ( (rt.rect.width / 2)*(-1), rt.rect.height/ 2, 0);	
				}
				if (rt.pivot.y == 1) {
					bc.center = new Vector3 ( (rt.rect.width  / 2), ( rt.rect.height / 2)*(-1), 0);	
				}

			}
			bc.size = new Vector3(rt.rect.width, rt.rect.height, 0.1f);
		//}catch (MissingComponentException e){
		//}
	}
}
