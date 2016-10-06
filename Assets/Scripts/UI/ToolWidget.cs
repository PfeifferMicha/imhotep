using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToolWidget : MonoBehaviour {

	// Use this for initialization
	void Start () {
		transform.SetParent (ToolUIAnchor.instance.transform, false);

		// Set material for all texts:
		Material mat = new Material(Shader.Find("Custom/TextShader"));
		Component[] texts;
		texts = GetComponentsInChildren( typeof(Text), true );

		if( texts != null )
		{
			foreach (Text t in texts)
				t.material = mat;
		}

		Material material = new Material(Shader.Find("Custom/UIObject"));
		material.renderQueue += 1;	// overlay!
		Component[] images;
		images = GetComponentsInChildren( typeof(Image), true );

		if( images != null )
		{
			foreach (Image i in images)
				i.material = material;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
