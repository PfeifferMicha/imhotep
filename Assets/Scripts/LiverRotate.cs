using UnityEngine;
using System.Collections;

public class LiverRotate : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (this.transform)
        {
            this.transform.Rotate(0, 10f * Time.deltaTime, 0);
            Vector3 pos = this.transform.position;
            this.transform.position = new Vector3(pos.x, Mathf.Sin(Time.time * 0.5f) * 15, pos.z);
        }
	}
}
