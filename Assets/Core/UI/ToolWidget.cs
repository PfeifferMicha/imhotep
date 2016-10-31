using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToolWidget : MonoBehaviour {

	// Use this for initialization
	void Start () {

		// Set material for all texts:
		Material mat = new Material(Shader.Find("Custom/TextShader"));
		mat.renderQueue += 1;	// overlay!
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

	public void OnEnable()
	{
		// Move the object to the current anchor (to "helmet" or to controller)
		Invoke ("MoveToUIAnchor",0.0001f);
	}

	public void OnDisable()
	{
		// Move the object back to the toolControl:
		Invoke ("MoveBackToToolControl",0.0001f);
	}

	private void MoveToUIAnchor()
	{
		transform.SetParent (ToolUIAnchor.instance.transform, false);
	}
	private void MoveBackToToolControl()
	{
		transform.SetParent (ToolControl.instance.transform, false);
	}
}
