using UnityEngine;
using System.Collections;


// This script changes the opacity of all chrildren containing a mesh renderer
public class MeshMaterialControl : MonoBehaviour {

	//private Shader meshShader, meshShaderTransparent;
	private Material materialOpaque, materialTransparent;

	public Color materialColor;

	// Use this for initialization
	void Awake() {
		//meshShader = Shader.Find("Custom/MeshShader");
		//meshShaderTransparent = Shader.Find("Custom/MeshShaderTransparent");
		materialOpaque = Instantiate( Resources.Load("Materials/MeshOpaque", typeof(Material)) as Material ) as Material;
		materialOpaque.SetFloat ("_amount", 0f);
		materialTransparent = Instantiate( Resources.Load("Materials/MeshTransparent", typeof(Material)) as Material ) as Material;
		materialTransparent.SetFloat ("_amount", 0f);
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
			Material mat;
			if(f == 1.0f) //Use opaque material
			{
				//mat.shader = meshShader;
				mat = materialOpaque;
			}
			else
			{
				//mat.shader = meshShaderTransparent;
				mat = materialTransparent;
			}
			//Material mat = mr.material;
			//mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, f);


			mat.color = new Color (materialColor.r, materialColor.g, materialColor.b, f);
			mr.material = mat;
		}
	}

	public void SetLoadingEffectAmount( float amount )
	{
		if( materialTransparent != null )
			materialTransparent.SetFloat ("_amount", amount);
		if( materialOpaque != null )
			materialOpaque.SetFloat ("_amount", amount);
	}
}
