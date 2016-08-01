using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OpacitySlider : MonoBehaviour
{
	
	public GameObject gameObjectToChangeOpacity;
	public Shader meshShader, meshShaderTransparent;

    // Use this for initialization
    void Start()
	{
		meshShader = Shader.Find("Custom/MeshShader");
		meshShaderTransparent = Shader.Find("Custom/MeshShaderTransparent");

		//mat = Resources.Load("Materials/DefaultMaterialAfterLoadingTransparent", typeof(Material)) as Material;
		//meshShaderTransparent = mat.shader;
    }

	void OnEnable()
	{
		// Register event callbacks:
		PatientEventSystem.startListening(PatientEventSystem.Event.MESH_Opacity_Changed, updateSlider);
	}

	void OnDisable()
	{
		// Unregister myself:
		PatientEventSystem.stopListening(PatientEventSystem.Event.MESH_Opacity_Changed, updateSlider);
	}

    public void changeOpacity(float f)
    {
		if (gameObjectToChangeOpacity == null)
			return;

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

	// Called if Silder value changed from external tool.
	private void updateSlider(object obj = null){		
		if (gameObjectToChangeOpacity != null) {
			float currentOpacity = 0f;
			if (gameObjectToChangeOpacity.activeSelf) {
				MeshRenderer mr = gameObjectToChangeOpacity.GetComponentInChildren<MeshRenderer> ();
				currentOpacity = mr.material.color.a;
			} else {
				currentOpacity = 0f;
			}
			Debug.Log (currentOpacity + " "  + GetComponent<Slider> ().value);
			if (GetComponent<Slider> ().value != currentOpacity) {
				GetComponent<Slider> ().value = currentOpacity;
			}
		}
	}

	void Update()
	{
		
	}
}
