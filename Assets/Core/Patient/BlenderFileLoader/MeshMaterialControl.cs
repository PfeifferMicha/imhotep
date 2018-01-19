using UnityEngine;
using System.Collections;


// This script changes the opacity of all chrildren containing a mesh renderer
public class MeshMaterialControl : MonoBehaviour {

	//private Shader meshShader, meshShaderTransparent;
	private Material materialOpaque, materialTransparent, materialHologram;

	private Color materialColor;

	public void setColor( Color color )
	{
		materialColor = color;
		materialOpaque.color = color;
		materialTransparent.color = color;
		materialHologram.color = color;
	}

	bool loadingEffectActive = false;
	float loadingAmount = 0;

	// Use this for initialization
	void Awake() {
		//meshShader = Shader.Find("Custom/MeshShader");
		//meshShaderTransparent = Shader.Find("Custom/MeshShaderTransparent");
		materialHologram = Instantiate( Resources.Load("Materials/MeshHologram", typeof(Material)) as Material ) as Material;
		materialOpaque = Instantiate( Resources.Load("Materials/MeshOpaque", typeof(Material)) as Material ) as Material;
		materialTransparent = Instantiate( Resources.Load("Materials/MeshTransparent", typeof(Material)) as Material ) as Material;
	}

	private Bounds calculateBoundingBox()
	{
		Bounds b = new Bounds ();
		bool initialized = false;

		foreach (Transform tf in transform)
		{
			MeshFilter mf = tf.GetComponent<MeshFilter> ();
			if (mf) {
				if (!initialized) {
					b = new Bounds (mf.mesh.bounds.center, mf.mesh.bounds.size);
					initialized = true;
				} else {
					b.Encapsulate (mf.mesh.bounds);
				}
			}
		}
		return b;
	}

	public void changeOpactiyOfChildren(float f){
		if (f <= 0.0f)
		{
			gameObject.SetActive(false);
			return;
		}
		gameObject.SetActive(true);

		Material mat;
		if(f >= 1.0f) //Use opaque material
			mat = materialOpaque;
		else
			mat = materialTransparent;

		mat.color = new Color (materialColor.r, materialColor.g, materialColor.b, f);

		foreach (Transform tf in transform)
		{
			MeshRenderer mr = tf.GetComponent<MeshRenderer> ();
			if( mr )
				mr.material = mat;
		}
	}

	public void startLoadingEffect()
	{
		endLoadingEffect ();	// Clean up from possible earlier loading effects
		SetLoadingEffectAmount (0);

		GameObject loadingEffectGameObject = new GameObject ("LoadingEffect");

		foreach (Transform tf in transform) {
			tf.gameObject.SetActive (true);
			GameObject clone = Object.Instantiate (tf.gameObject);
			clone.GetComponent<MeshRenderer> ().material = materialHologram;
			clone.transform.SetParent (loadingEffectGameObject.transform, false);
			Destroy (clone.GetComponent<MeshCollider> ());
		}
		loadingEffectGameObject.transform.SetParent (transform, false);

		loadingEffectActive = true;
		loadingAmount = 0;
	}

	public void endLoadingEffect()
	{
		loadingEffectActive = false;
		Transform tf = transform.Find ("LoadingEffect");
		if( tf != null )
			Destroy (tf.gameObject);
	}

	public void SetLoadingEffectAmount( float amount )
	{
		Bounds b = calculateBoundingBox ();
		
		amount = Mathf.Clamp (amount, 0f, 5f);
		materialHologram.SetFloat ("_amount", amount);
		materialHologram.SetVector ("_size", b.size);
		materialHologram.SetVector ("_center", b.center);

		materialOpaque.SetFloat ("_amount", amount - 3f );
		materialOpaque.SetVector ("_size", b.size);
		materialOpaque.SetVector ("_center", b.center);

		materialTransparent.SetFloat ("_amount", amount - 3f );
		materialTransparent.SetVector ("_size", b.size);
		materialTransparent.SetVector ("_center", b.center);
	}

	void Update () {
		if (loadingEffectActive) {
			loadingAmount = loadingAmount + 2f*Time.deltaTime;
			SetLoadingEffectAmount (loadingAmount);
			if (loadingAmount > 5) {
				endLoadingEffect ();
			}
		}
	}
}
