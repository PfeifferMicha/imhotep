using UnityEngine;
using BlenderMeshReader;
using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using System.IO;

public class MeshLoader : MonoBehaviour {

    public GameObject meshNode;
    public Material defaultMaterial;

    //List of lists of UnityMeshes with max. 2^16 vertices per mesh
    private volatile List<List<UnityMesh>> unityMeshes = new List<List<UnityMesh>>();
    //True if file is loaded
    private bool loaded = false;
	private bool triggerEvent = false;
    private volatile string Path = "";
    //Contains a list of game objects. This game objects are parents of actual meshs. 
    public List<GameObject> MeshGameObjectContainers { get; set; }


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

			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.PATIENT_LoadingProcess,
				"Mesh: Finished loading.\n");

        }
		if(triggerEvent){
			triggerEvent = false;
			PatientEventSystem.triggerEvent(PatientEventSystem.Event.MESH_Loaded);
		}
    }

    public void LoadFile(string path)
    {
		if (File.Exists (path)) {

			// Let loading screen know what we're currently doing:
			PatientEventSystem.triggerEvent (PatientEventSystem.Event.PATIENT_LoadingProcess,
				"Mesh: Started loading...\n");
			
            this.RemoveMesh();
			this.Path = path;
            MeshGameObjectContainers = new List<GameObject>();

			ThreadUtil t = new ThreadUtil (this.LoadFileWorker, this.LoadFileCallback);
			t.Run ();
		} else {
			Debug.LogWarning ("Could not load mesh from: " + path + ", file not found.");
		}
        
        //Thread thread = new Thread(new ThreadStart(this.LoadFileWorker));
        //thread.Start();
        //LoadFileWorker();
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
        BackgroundWorker worker = sender as BackgroundWorker;
        if (e.Cancelled)
        {
            Debug.Log("Loading cancelled");
        }else if (e.Error != null)
		{
            Debug.LogError("[Loader.cs] Error while loading the mesh");
        }
        else
		{
            loaded = true;
        }
        return;
    }

    private IEnumerator LoadFileExecute()
    {
        foreach (List<UnityMesh> um in unityMeshes) {

            GameObject containerObject = new GameObject(um[0].Name);
            containerObject.layer = meshNode.layer; //Set same layer as parent
            containerObject.transform.parent = meshNode.transform;
            containerObject.transform.localPosition = new Vector3(0, 0, 0);
            MeshGameObjectContainers.Add(containerObject);

            foreach (UnityMesh unityMesh in um)
            {
                //Spawn object
                GameObject objToSpawn = new GameObject(unityMesh.Name);
                objToSpawn.layer = meshNode.layer; //Set same layer as parent

                objToSpawn.transform.parent = containerObject.transform;


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
                objToSpawn.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

                unityMeshes = new List<List<UnityMesh>>();
                loaded = false;
                Path = "";

				Material mat = matForMeshName (mesh.name);
				Debug.Log (mat);
				if (mat != null) {
					var materials = objToSpawn.GetComponent<MeshRenderer> ().materials;
					materials [0] = mat;
					objToSpawn.GetComponent<MeshRenderer> ().materials = materials;
				}

                yield return null;
            }
            
        }
		triggerEvent = true;

        yield return null;
    }

    public void RemoveMesh()
    {
        //Destroy current game objectes attached to mesh node
        for (int i = 0; i < meshNode.transform.childCount; i++)
        {
            Destroy(meshNode.transform.GetChild(i).gameObject);
        }
    }

	private Material matForMeshName( string meshName )
	{
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

		return null;
	}

}
