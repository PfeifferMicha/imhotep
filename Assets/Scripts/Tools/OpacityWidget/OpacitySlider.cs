using UnityEngine;
using System.Collections;

public class OpacitySlider : MonoBehaviour {

    public GameObject gameObjectToChangeOpacity { get; set; }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void changeOpacity(float f)
    {
        Debug.Log(f);
        foreach(MeshRenderer mr in gameObjectToChangeOpacity.GetComponentsInChildren<MeshRenderer>())
        {
            mr.material.color = new Color(mr.material.color.r, mr.material.color.g, mr.material.color.b, f);
            //TODO How to change alpha???
        }
    }
}
