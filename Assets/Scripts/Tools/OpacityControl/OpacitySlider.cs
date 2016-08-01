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
		MeshOpacityChanger moc = gameObjectToChangeOpacity.GetComponent<MeshOpacityChanger> ();
		if (moc != null) {
			moc.changeOpactiyOfChildren (f);
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
			if (GetComponent<Slider> ().value != currentOpacity) {
				GetComponent<Slider> ().value = currentOpacity;
			}
		}
	}

	void Update()
	{
		
	}
}
