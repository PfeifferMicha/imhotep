using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class OpacityControl : MonoBehaviour {

    public GameObject defaultLine;

    private MeshLoader mMeshLoader;

    // Use this for initialization
    void Start () {
        mMeshLoader = GameObject.Find("GlobalScript").GetComponent<MeshLoader>();
        defaultLine.SetActive(false);
        createContent(); //TODO dont draw on start, use patient events
	}
	
	// Update is called once per frame
	void Update () {
    }

    private void createContent()
    {        
        foreach(GameObject g in mMeshLoader.MeshGameObjectContainers)
        {
            // Create a new instance of the list button:
            GameObject newLine = Instantiate(defaultLine).gameObject;
            newLine.SetActive(true);

            // Attach the new button to the list:
            newLine.transform.SetParent(defaultLine.transform.parent, false);

            //Save game object in slider
            GameObject slider = newLine.transform.Find("Slider").gameObject;
            slider.GetComponent<OpacitySlider>().gameObjectToChangeOpacity = g;

            // Change button text to name of tool:
            GameObject textObject = newLine.transform.Find("Text").gameObject;
            Text buttonText = textObject.GetComponent<Text>();
            buttonText.text = g.name;
   
        }
    }
}
