using UnityEngine;
using System.Collections;

public class FootPrints : MonoBehaviour {

	private GameObject text;
	public GameObject Camera;

	void Start()
	{
		text = transform.Find ("PleaseStandHere").gameObject;
	}

	// Update is called once per frame
	void Update () {
		if (text != null) {
			if (Time.frameCount > 5 && Time.time > 2) {
				if ((text.transform.position - Camera.transform.position).magnitude < 0.35f) {
					GameObject.Destroy (text);
					text = null;
					Platform.instance.activateUIMesh ();
				}
			}
		}
	}
}
