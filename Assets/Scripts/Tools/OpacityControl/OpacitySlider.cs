using UnityEngine;
using System.Collections;

public class OpacitySlider : MonoBehaviour
{

	public GameObject gameObjectToChangeOpacity { get; set; }
	public Shader meshShader;
	public Shader meshShaderTransparent;

    // Use this for initialization
    void Start()
	{
		meshShader = Shader.Find("Custom/MeshShader");

		//mat = Resources.Load("Materials/DefaultMaterialAfterLoadingTransparent", typeof(Material)) as Material;
		//meshShaderTransparent = mat.shader;
		meshShaderTransparent = Shader.Find("Custom/MeshShaderTransparent");
    }

    public void changeOpacity(float f)
    {
        //Debug.Log(f);

        if (f == 0.0f)
        {
            gameObjectToChangeOpacity.SetActive(false);
            return;
        }
        else
        {
            gameObjectToChangeOpacity.SetActive(true);
        }

        foreach (MeshRenderer mr in gameObjectToChangeOpacity.GetComponentsInChildren<MeshRenderer>())
        {
			Material mat = mr.material;
            if(f == 1.0f) //Use opaque material
			{
				mat.shader = meshShader;
                mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, f);

				//Material mat = Resources.Load("Materials/DefaultMaterialAfterLoadingOpaque", typeof(Material)) as Material;
                //mr.material = new Material(mat);
            }
            else if(f > 0.0 && f < 1.0f) // Use transparent material
			{
				mat.shader = meshShaderTransparent;
				mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, f);
                //Material mat = Resources.Load("Materials/DefaultMaterialAfterLoadingTransparent", typeof(Material)) as Material;
                //mat.color = new Color(mr.material.color.r, mr.material.color.g, mr.material.color.b, f);
                //mr.material = new Material(mat); 
            }

        }
    }
}
