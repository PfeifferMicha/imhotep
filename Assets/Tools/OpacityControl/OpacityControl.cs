using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class OpacityControl : MonoBehaviour
{

    public GameObject defaultLine;

    private MeshLoader mMeshLoader;

    void OnEnable()
    {
        // Register event callbacks for MESH events:
        PatientEventSystem.startListening(PatientEventSystem.Event.MESH_LoadedAll, createContent);
        PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_Closed, clearContent); 
        
		mMeshLoader = GameObject.Find("GlobalScript").GetComponent<MeshLoader>();
		defaultLine.SetActive(false);
		if (mMeshLoader.MeshGameObjectContainers.Count != 0)
		{
			createContent();
		}
    }

    void OnDisable()
    {
        // Unregister myself - no longer receives events (until the next OnEnable() call):
        PatientEventSystem.stopListening(PatientEventSystem.Event.MESH_LoadedAll, createContent);
        PatientEventSystem.stopListening(PatientEventSystem.Event.PATIENT_Closed, clearContent);
    }



    private void createContent(object obj = null)
    {
        clearContent();

        foreach (GameObject g in mMeshLoader.MeshGameObjectContainers)
        {
            // Create a new instance of the list button:
            GameObject newLine = Instantiate(defaultLine).gameObject;
            newLine.SetActive(true);

            // Attach the new button to the list:
            newLine.transform.SetParent(defaultLine.transform.parent, false);

            //Save game object in slider
            GameObject slider = newLine.transform.Find("Slider").gameObject;
			slider.GetComponent<OpacitySlider> ().gameObjectToChangeOpacity = g;

            // Change button text to name of tool:
            GameObject textObject = newLine.transform.Find("Text").gameObject;
            Text buttonText = textObject.GetComponent<Text>();
			if (g.name.Substring (0, 2) == "ME") {
				buttonText.text = g.name.Substring (2, g.name.Length - 2);
			} else {
				buttonText.text = g.name;
			}
        }
    }

    private void clearContent(object obj = null)
    {
        //Destroy all object except for default line
        for (int i = 0; i < defaultLine.transform.parent.childCount; i++)
        {
            if (i != 0) //TODO !=0
            {
                Destroy(defaultLine.transform.parent.GetChild(i).gameObject);
            }
        }
    }

}
