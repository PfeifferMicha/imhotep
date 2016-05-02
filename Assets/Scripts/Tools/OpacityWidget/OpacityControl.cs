using UnityEngine;
using System.Collections;

public class OpacityControl : MonoBehaviour {

    public GameObject defaultLine;

	// Use this for initialization
	void Start () {
        defaultLine.SetActive(false);
        drawContent();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void drawContent()
    {
        for (int i = 0; i < 5; i++)
        {
            // Create a new instance of the list button:
            GameObject newLine = Instantiate(defaultLine).gameObject;
            newLine.SetActive(true);

            // Attach the new button to the list:
            newLine.transform.SetParent(defaultLine.transform.parent, false);

            /*
            // Change button text to name of tool:
            GameObject textObject = newButton.transform.Find("OverlayImage/Text").gameObject;
            Text buttonText = textObject.GetComponent<Text>();

            AnnotationPoint ap = g.GetComponent<AnnotationPoint>();
            if (ap != null)
            {
                buttonText.text = ap.text;
            }*/

        }
    }
}
