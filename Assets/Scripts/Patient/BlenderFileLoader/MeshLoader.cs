using UnityEngine;
using BlenderMeshReader;
using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using System.IO;
using LitJson;


public class MeshLoader : MonoBehaviour {

	public GameObject meshNode;
    public Material defaultMaterial;

    //List of lists of UnityMeshes with max. 2^16 vertices per mesh
    private volatile List<List<UnityMesh>> unityMeshes = new List<List<UnityMesh>>();    
    //True if file is loaded
    private bool loaded = false;
	private bool triggerEvent = false;
    private volatile string Path = "";

    private MeshJson meshJson = null;

    //Contains a list of game objects. This game objects are parents of actual meshs. 
    public List<GameObject> MeshGameObjectContainers { get; set; }


	void OnEnable()
	{
		// Register event callbacks:
		PatientEventSystem.startListening(PatientEventSystem.Event.PATIENT_Closed, RemoveMesh);
	}

	void OnDisable()
	{
		// Unregister myself:
		PatientEventSystem.stopListening(PatientEventSystem.Event.PATIENT_Closed, RemoveMesh);
	}


    // Use this for initialization
    void Start () {
        MeshGameObjectContainers = new List<GameObject>();
    }
	
	// Update is called once per frame
	void Update () {

		if (loaded)
        {
            StartCoroutine("LoadFileExecute");
            unityMeshes = new List<List<UnityMesh>>();
            loaded = false;
            Path = "";

        }
		if(triggerEvent){
            meshJson = null;
			triggerEvent = false;

			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_RemoveLoadingJob,	"Mesh");			
			PatientEventSystem.triggerEvent(PatientEventSystem.Event.MESH_LoadedAll);
		}
    }

    public void LoadFile(string pathToMeshJson)
    {
        //Check if mesh.json exists
        if (!File.Exists(pathToMeshJson))
        {
            Debug.LogWarning("Could not load mesh from: " + pathToMeshJson + ", file not found.");
            return;
        }

        // Read mesh.json
        string fileContent = "";
        string line;
        System.IO.StreamReader file = new System.IO.StreamReader(pathToMeshJson);
        while ((line = file.ReadLine()) != null)
        {
            fileContent += line;
        }
        file.Close();
        meshJson = JsonMapper.ToObject<MeshJson>(fileContent);
        if(meshJson == null)
        {
            Debug.LogWarning("Error while parsing mesh.json");
            return;
        }
        Patient currentPatient = Patient.getLoadedPatient();
        string path = currentPatient.path + "/" + meshJson.pathToBlendFile;

        //Loading blend file
        if (File.Exists (path)) {

			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.LOADING_AddLoadingJob, "Mesh");			
            this.RemoveMesh();
			this.Path = path;
			MeshGameObjectContainers = new List<GameObject>();

			// Reset scale, rotation, position:
			meshNode.transform.localScale = new Vector3 (0.007f, 0.007f, 0.007f);
			meshNode.transform.parent.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			meshNode.transform.parent.localRotation = Quaternion.identity;
			//meshNode.transform.parent.Rotate (90, 0, 0);
			meshNode.transform.parent.GetComponent<ModelRotator>().setTargetOrientation( Quaternion.Euler(90,0,0), 0.01f );

			ThreadUtil t = new ThreadUtil (this.LoadFileWorker, this.LoadFileCallback);
			t.Run ();
		} else {
			Debug.LogWarning ("Could not load mesh from: '" + path + "', file not found.");
		}
    }

    //Runs in own thread
    private void LoadFileWorker(object sender, DoWorkEventArgs e)
    {
        BlenderFile b = new BlenderFile(Path);
        List<BlenderMesh> blenderMeshes = new List<BlenderMesh>();
        blenderMeshes = b.readMesh();
        unityMeshes = BlenderFile.createSubmeshesForUnity(blenderMeshes);
        return;
    }


    private void LoadFileCallback(object sender, RunWorkerCompletedEventArgs e)
    {        
        //BackgroundWorker worker = sender as BackgroundWorker;
        if (e.Cancelled)
        {
            Debug.Log("Loading cancelled");
        }else if (e.Error != null)
		{
            Debug.LogError("[MeshLoader.cs] Error while loading the mesh");
        }
        else
		{
            loaded = true;
        }
        return;
    }

    private IEnumerator LoadFileExecute()
	{
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (List<UnityMesh> um in unityMeshes) {

            GameObject containerObject = new GameObject(um[0].Name);
            containerObject.layer = meshNode.layer; //Set same layer as parent
			containerObject.transform.SetParent( meshNode.transform, false );
			//containerObject.transform.localScale = new Vector3 (400.0f, 400.0f, 400.0f);
            containerObject.transform.localPosition = new Vector3(0, 0, 0);
            MeshGameObjectContainers.Add(containerObject);

            foreach (UnityMesh unityMesh in um)
            {
                //Spawn object
                GameObject objToSpawn = new GameObject(unityMesh.Name);
                objToSpawn.layer = meshNode.layer; //Set same layer as parent

				objToSpawn.transform.SetParent (containerObject.transform, false);
				 
                //Add Components
                objToSpawn.AddComponent<MeshFilter>();
                objToSpawn.AddComponent<MeshCollider>(); //TODO need to much time --> own thread?? Dont work in Unity!!
                objToSpawn.AddComponent<MeshRenderer>();
                
                //Add material
                objToSpawn.GetComponent<MeshRenderer>().material = defaultMaterial;
               
                //Create Mesh
                Mesh mesh = new Mesh();
                mesh.name = unityMesh.Name;
                mesh.vertices = unityMesh.VertexList;
                mesh.normals = unityMesh.NormalList;
                mesh.triangles = unityMesh.TriangleList;

                objToSpawn.GetComponent<MeshFilter>().mesh = mesh;
                objToSpawn.GetComponent<MeshCollider>().sharedMesh = mesh; //TODO Reduce mesh??

                objToSpawn.transform.localPosition = new Vector3(0, 0, 0);

                unityMeshes = new List<List<UnityMesh>>();
                loaded = false;
                Path = "";

				Material mat = new Material (matForMeshName (mesh.name));
				if (mat != null) {
					//var materials = objToSpawn.GetComponent<MeshRenderer> ().materials;
					//materials [0] = mat;
					objToSpawn.GetComponent<Renderer> ().material = mat;
				
					/*float min = mat.GetFloat ("_min");
					float max = mat.GetFloat ("_max");
					mat.SetFloat ("_min", Math.Min( mesh.bounds.min.z, min ));
					mat.SetFloat ("_max", Math.Max( mesh.bounds.max.z, max ));
					mat.SetFloat ("_amount", -1f );*/		// Default: inactive.
					//objToSpawn.GetComponent<MeshRenderer> ().materials = materials;
				}


				// Increase the common bounding box to contain this object:
				bounds.Encapsulate (mesh.bounds.min);
				bounds.Encapsulate (mesh.bounds.max);

				// Let others know that a new mesh has been loaded:
				PatientEventSystem.triggerEvent (PatientEventSystem.Event.MESH_LoadedSingle, objToSpawn);

                yield return null;
            }

			// Move the object by half the size of all of the meshes.
			// This makes sure the object will rotate around its actual center:
			//containerObject.transform.localPosition = -bounds.center;

			meshNode.GetComponent<ModelMover> ().targetPosition = Vector3.Scale(-bounds.center, meshNode.transform.localScale);
            
        }


		triggerEvent = true;

        yield return null;
    }

    public void RemoveMesh(object obj = null)
    {
        //Destroy current game objectes attached to mesh node
        for (int i = 0; i < meshNode.transform.childCount; i++)
        {
            Destroy(meshNode.transform.GetChild(i).gameObject);
        }
    }

	public Material matForMeshName( string meshName )
	{
		foreach(MeshListElement mle in meshJson.meshList)
		{
			if(mle.name == meshName)
			{
				Debug.Log("Found color for " + mle.name + ": " + mle.color);
				Material mat = Resources.Load("Materials/DefaultMud", typeof(Material)) as Material;
                //Material mat = Resources.Load("Materials/DefaultMaterialAfterLoadingOpaque", typeof(Material)) as Material;
				mat.color = HexToColor (mle.color);
				return mat;
			}
		}

        /*Debug.Log("Found no color for " + meshName + ". Use default color");
        Material mat1 = Resources.Load("Materials/DefaultMaterialAfterLoadingOpaque", typeof(Material)) as Material;
        mat1.color = HexToColor("#808080");
        return mat1;*/

        
		bool contains;

		contains = meshName.IndexOf("tumor", StringComparison.OrdinalIgnoreCase) >= 0;
		if( contains )
		{
			return Resources.Load("Materials/Tumor", typeof(Material)) as Material;
		}

		contains = meshName.IndexOf("liver", StringComparison.OrdinalIgnoreCase) >= 0;
		if( contains )
		{
			return Resources.Load("Materials/Liver", typeof(Material)) as Material;
		}

		contains = meshName.IndexOf ("artery", StringComparison.OrdinalIgnoreCase) >= 0 ||
			meshName.IndexOf ("arteries", StringComparison.OrdinalIgnoreCase) >= 0;
		if( contains )
		{
			return  (Material)Resources.Load("Materials/Arteries", typeof(Material));
		}

		contains = meshName.IndexOf ("vein", StringComparison.OrdinalIgnoreCase) >= 0;
		if( contains )
		{
			return  (Material)Resources.Load("Materials/VenaCava", typeof(Material));
		}

		contains = meshName.IndexOf ("gall", StringComparison.OrdinalIgnoreCase) >= 0;
		if( contains )
		{
			return  (Material)Resources.Load("Materials/Gallbladder", typeof(Material));
		}

		contains = meshName.IndexOf ("pancreas", StringComparison.OrdinalIgnoreCase) >= 0;
		if( contains )
		{
			return  (Material)Resources.Load("Materials/Pancreas", typeof(Material));
		}

		contains = meshName.IndexOf("spleen", StringComparison.OrdinalIgnoreCase) >= 0;
		if( contains )
		{
			return Resources.Load("Materials/Spleen", typeof(Material)) as Material;
		}

		contains = meshName.IndexOf("kidney", StringComparison.OrdinalIgnoreCase) >= 0;
		if( contains )
		{
			return Resources.Load("Materials/Kidney", typeof(Material)) as Material;
		}

		contains = meshName.IndexOf("bone", StringComparison.OrdinalIgnoreCase) >= 0;
		if( contains )
		{
			return Resources.Load("Materials/Bone", typeof(Material)) as Material;
		}

		contains = meshName.IndexOf("ventricles", StringComparison.OrdinalIgnoreCase) >= 0;
		if( contains )
		{
			return Resources.Load("Materials/Ventricles", typeof(Material)) as Material;
		}

		return Resources.Load("Materials/DefaultMud", typeof(Material)) as Material;
    }

	// Adopted rom the Unity Wiki:
	// http://wiki.unity3d.com/index.php?title=HexConverter
	public static string ColorToHex(Color32 color)
	{
		string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		return hex;
	}

	// Adopted rom the Unity Wiki:
	// http://wiki.unity3d.com/index.php?title=HexConverter
	public static Color HexToColor(string hex)
	{
        //Debug.LogWarning(hex);
		byte r = byte.Parse(hex.Substring(1,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(3,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(5,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r,g,b, 255);
	}


}
