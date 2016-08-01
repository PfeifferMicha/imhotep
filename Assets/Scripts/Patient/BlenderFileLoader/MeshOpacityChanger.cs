using UnityEngine;
using System.Collections;


// This script changes the opacity of all chrildren containing a mesh renderer
public class MeshOpacityChanger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void changeOpactiyOfChildren(float f){
		
		if (f == 0.0f)
		{
			this.gameObject.SetActive(false);
			return;
		}
		else
		{
			this.gameObject.SetActive(true);
		}

		foreach (MeshRenderer mr in this.gameObject.GetComponentsInChildren<MeshRenderer>())
		{
			//Material mat = mr.material;
			if(f == 1.0f) //Use opaque material
			{
				//mat.shader = meshShader;
				//mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, f);

				Material mat = Resources.Load("Materials/DefaultMaterialAfterLoadingOpaque", typeof(Material)) as Material;
				mat.color = new Color(mr.material.color.r, mr.material.color.g, mr.material.color.b, f);
				mr.material = new Material(mat);
			}
			else
			{
				//mat.shader = meshShaderTransparent;
				//mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, f);
				Material mat = Resources.Load("Materials/DefaultMaterialAfterLoadingTransparent", typeof(Material)) as Material;
				mat.color = new Color(mr.material.color.r, mr.material.color.g, mr.material.color.b, f);
				mr.material = new Material(mat); 
			}
		}
	}

}
