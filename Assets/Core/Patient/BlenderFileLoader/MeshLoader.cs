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

    //List of lists of UnityMeshes with max. 2^16 vertices per mesh
    private volatile List<List<UnityMesh>> unityMeshes = new List<List<UnityMesh>>();
    //List of blender objects, witch conatins name, location and rotation of all blender objects
    private volatile List<BlenderObjectBlock> blenderObjects = new List<BlenderObjectBlock>();
    //True if file is loaded
    private bool loaded = false;
	private bool triggerEvent = false;
    private volatile string Path = "";

    private MeshJson meshJson = null;

    //Contains a list of game objects. This game objects are parents of actual meshs. 
    public List<GameObject> MeshGameObjectContainers { get; set; }

	public MeshLoader () {
		MeshGameObjectContainers = new List<GameObject>();
	}

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
            blenderObjects = new List<BlenderObjectBlock>();
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
			meshNode.transform.parent.GetComponent<ModelRotator>().setTargetOrientation( Quaternion.Euler(90,0,0) );

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
        blenderObjects = b.readObject();
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
			Debug.LogError("[MeshLoader.cs] Loading error: " + e.Error.Message);
        }
        else
		{
            loaded = true;
        }
        return;
    }

    private IEnumerator LoadFileExecute()
	{
		Bounds bounds = new Bounds ();
		bool boundsInitialized = false; // set to true when bounds is first set

        //  meshNode
        //	| - containerObject
        //		| - actual mesh
        //		| - actual mesh
        //		| - actual mesh
        //		| - ...
        //	| - containerObject
        //		| - actual mesh
        //		| - actual mesh
        //		| - actual mesh
        //		| - ...
        //	| ...

        foreach (List<UnityMesh> um in unityMeshes) {
            GameObject containerObject = new GameObject(um[0].Name);
            containerObject.layer = meshNode.layer; //Set same layer as parent
			containerObject.transform.SetParent( meshNode.transform, false );
			//containerObject.transform.localScale = new Vector3 (400.0f, 400.0f, 400.0f);
            containerObject.transform.localPosition = new Vector3(0, 0, 0);
			MeshMaterialControl matControl = containerObject.AddComponent<MeshMaterialControl> ();
			Color col = matColorForMeshName (um[0].Name);
			matControl.materialColor = col;
            MeshGameObjectContainers.Add(containerObject);

            //attach BlenderObject to containerObject
            foreach(BlenderObjectBlock b in blenderObjects)
            {
                if (b.uniqueIdentifier == um[0].UniqueIdentifier) 
                {
                    BlenderObject attachedObject = containerObject.AddComponent<BlenderObject>(); //TODO Remove... maybe
                    attachedObject.objectName = b.objectName;
                    attachedObject.location = b.location;
                    attachedObject.rotation = b.rotation;

                    //Convert to left-handed soordinate systems
                    //containerObject.transform.localPosition = new Vector3(b.location.x, b.location.y, -b.location.z);

                    /* TODO
                    Quaternion rot = Quaternion.Inverse(b.rotation);
                    rot = new Quaternion(-rot.x, -rot.z, rot.y, -rot.w);
                    containerObject.transform.localRotation = rot;
                    */

                }
            }

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

				// Increase the common bounding box to contain this object:
				if (!boundsInitialized) {
					bounds = mesh.bounds;
					boundsInitialized = true;
				} else {
					bounds.Encapsulate (mesh.bounds);
				}

				// Let others know that a new mesh has been loaded:
				PatientEventSystem.triggerEvent (PatientEventSystem.Event.MESH_LoadedSingle, objToSpawn);

				// Make sure the color of the material is set correctly:
				matControl.changeOpactiyOfChildren (1f);

				// Deactivate for now, let someone else activate the mesh when needed:
				objToSpawn.SetActive( false );

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
        //Destroy current game objects attached to mesh node
        for (int i = 0; i < meshNode.transform.childCount; i++)
        {
			if (meshNode.transform.GetChild (i).name != "DICOMBounds") {
				Destroy (meshNode.transform.GetChild (i).gameObject);
			}
        }
    }

	public Color matColorForMeshName( string meshName )
	{
		foreach(MeshListElement mle in meshJson.meshList)
		{
			if(mle.name == meshName)
			{
				return HexToColor (mle.color);
			}
		}
		return new Color (0.7f, 0.5f, 0.2f);
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

    /*public List<BlenderObject> getBlenderObjects()
    {
        return blenderObjects;
    }*/


}
