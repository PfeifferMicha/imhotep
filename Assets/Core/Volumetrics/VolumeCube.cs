using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VolumeCube : MonoBehaviour {

	public MeshFilter stackOfSlices;


	void OnEnable()
	{
		// Register event callbacks for all DICOM events:
		PatientEventSystem.startListening( PatientEventSystem.Event.DICOM_NewLoadedVolume, eventDisplayCurrentDicom );
		PatientEventSystem.startListening( PatientEventSystem.Event.PATIENT_Closed, eventClear );
		eventClear ();
		//eventDisplayCurrentDicom ();
		buildMesh();
	}

	void OnDisable()
	{
		// Unregister myself - no longer receive events (until the next OnEnable() call):
		PatientEventSystem.stopListening( PatientEventSystem.Event.DICOM_NewLoadedVolume, eventDisplayCurrentDicom );
		PatientEventSystem.stopListening( PatientEventSystem.Event.PATIENT_Closed, eventClear );
	}

	// Use this for initialization
	void Start () {
	}

	void buildMesh()  {

		float start_time = Time.time;

		List<Vector3> verts = new List<Vector3> ();
		List<int> tris = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		List<Vector3> normals = new List<Vector3> ();
		Vector3 normal;
		int vertsAlreadyAdded = 0;

		// TODO: Current volume starts at 0.5*sliceDelta, not at 0 as it should!
		int numSlices = 512;//512;
		float sliceDelta = 1f / (float)numSlices;
		normal = new Vector3 (0, 0, 1);
		vertsAlreadyAdded = verts.Count;
		for (int slice = 0; slice < numSlices; slice ++) {
			Vector3 vert1 = new Vector3 (-1, -1, -1 + 2f*(float)(slice+0.5f)*sliceDelta);
			Vector3 vert2 = new Vector3 (-1, 1, -1 + 2f*(float)(slice+0.5f)*sliceDelta);
			Vector3 vert3 = new Vector3 (1, -1, -1 + 2f*(float)(slice+0.5f)*sliceDelta);
			Vector3 vert4 = new Vector3 (1, 1, -1 + 2f*(float)(slice+0.5f)*sliceDelta);

			// Front faces:
			verts.Add (vert1);
			verts.Add (vert2);
			verts.Add (vert3);
			verts.Add (vert4);
			normals.Add (normal);
			normals.Add (normal);
			normals.Add (normal);
			normals.Add (normal);
			// 1
			tris.Add (vertsAlreadyAdded + slice*4 + 2);
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
			tris.Add (vertsAlreadyAdded + slice*4 + 0);
			// 2
			tris.Add (vertsAlreadyAdded + slice*4 + 2);
			tris.Add (vertsAlreadyAdded + slice*4 + 3);
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
		}
		vertsAlreadyAdded = verts.Count;
		for (int slice = 0; slice < numSlices; slice ++) {
			Vector3 vert1 = new Vector3 (-1, -1, 1 - 2f*(float)(slice+0.5f)*sliceDelta);
			Vector3 vert2 = new Vector3 (-1, 1, 1 - 2f*(float)(slice+0.5f)*sliceDelta);
			Vector3 vert3 = new Vector3 (1, -1, 1 - 2f*(float)(slice+0.5f)*sliceDelta);
			Vector3 vert4 = new Vector3 (1, 1, 1 - 2f*(float)(slice+0.5f)*sliceDelta);

			// Front faces:
			verts.Add (vert1);
			verts.Add (vert2);
			verts.Add (vert3);
			verts.Add (vert4);
			normals.Add (-normal);
			normals.Add (-normal);
			normals.Add (-normal);
			normals.Add (-normal);
			// 1
			tris.Add (vertsAlreadyAdded + slice*4 + 0);
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
			tris.Add (vertsAlreadyAdded + slice*4 + 2);
			// 2
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
			tris.Add (vertsAlreadyAdded + slice*4 + 3);
			tris.Add (vertsAlreadyAdded + slice*4 + 2);
		}

		normal = new Vector3 (0, 1, 0);
		vertsAlreadyAdded = verts.Count;
		for (int slice = 0; slice < numSlices; slice ++) {
			Vector3 vert1 = new Vector3 (-1, -1 + 2f*(float)(slice+0.5f)*sliceDelta, -1);
			Vector3 vert2 = new Vector3 (-1, -1 + 2f*(float)(slice+0.5f)*sliceDelta, 1);
			Vector3 vert3 = new Vector3 (1, -1 + 2f*(float)(slice+0.5f)*sliceDelta, -1);
			Vector3 vert4 = new Vector3 (1, -1 + 2f*(float)(slice+0.5f)*sliceDelta, 1);

			// Front faces:
			verts.Add (vert1);
			verts.Add (vert2);
			verts.Add (vert3);
			verts.Add (vert4);
			normals.Add (normal);
			normals.Add (normal);
			normals.Add (normal);
			normals.Add (normal);

			// 1
			tris.Add (vertsAlreadyAdded + slice*4 + 0);
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
			tris.Add (vertsAlreadyAdded + slice*4 + 2);
			// 2
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
			tris.Add (vertsAlreadyAdded + slice*4 + 3);
			tris.Add (vertsAlreadyAdded + slice*4 + 2);

		}
		vertsAlreadyAdded = verts.Count;
		for (int slice = 0; slice < numSlices; slice ++) {
			Vector3 vert1 = new Vector3 (-1, 1 - 2f*(float)(slice+0.5f)*sliceDelta, -1);
			Vector3 vert2 = new Vector3 (-1, 1 - 2f*(float)(slice+0.5f)*sliceDelta, 1);
			Vector3 vert3 = new Vector3 (1, 1 - 2f*(float)(slice+0.5f)*sliceDelta, -1);
			Vector3 vert4 = new Vector3 (1, 1 - 2f*(float)(slice+0.5f)*sliceDelta, 1);

			// Front faces:
			verts.Add (vert1);
			verts.Add (vert2);
			verts.Add (vert3);
			verts.Add (vert4);
			normals.Add (-normal);
			normals.Add (-normal);
			normals.Add (-normal);
			normals.Add (-normal);
			// 1
			tris.Add (vertsAlreadyAdded + slice*4 + 2);
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
			tris.Add (vertsAlreadyAdded + slice*4 + 0);
			// 2
			tris.Add (vertsAlreadyAdded + slice*4 + 2);
			tris.Add (vertsAlreadyAdded + slice*4 + 3);
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
		}

		normal = new Vector3 (1, 0, 0);
		vertsAlreadyAdded = verts.Count;
		for (int slice = 0; slice < numSlices; slice ++) {
			Vector3 vert1 = new Vector3 (-1 + 2f*(float)(slice+0.5f)*sliceDelta, -1, -1);
			Vector3 vert2 = new Vector3 (-1 + 2f*(float)(slice+0.5f)*sliceDelta, -1, 1);
			Vector3 vert3 = new Vector3 (-1 + 2f*(float)(slice+0.5f)*sliceDelta, 1, -1);
			Vector3 vert4 = new Vector3 (-1 + 2f*(float)(slice+0.5f)*sliceDelta, 1, 1);

			// Front faces:
			verts.Add (vert1);
			verts.Add (vert2);
			verts.Add (vert3);
			verts.Add (vert4);
			normals.Add (normal);
			normals.Add (normal);
			normals.Add (normal);
			normals.Add (normal);
			// 1
			tris.Add (vertsAlreadyAdded + slice*4 + 2);
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
			tris.Add (vertsAlreadyAdded + slice*4 + 0);
			// 2
			tris.Add (vertsAlreadyAdded + slice*4 + 2);
			tris.Add (vertsAlreadyAdded + slice*4 + 3);
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
		}
		vertsAlreadyAdded = verts.Count;
		for (int slice = 0; slice < numSlices; slice ++) {
			Vector3 vert1 = new Vector3 (1 - 2f*(float)(slice+0.5f)*sliceDelta, -1, -1);
			Vector3 vert2 = new Vector3 (1 - 2f*(float)(slice+0.5f)*sliceDelta, -1, 1);
			Vector3 vert3 = new Vector3 (1 - 2f*(float)(slice+0.5f)*sliceDelta, 1, -1);
			Vector3 vert4 = new Vector3 (1 - 2f*(float)(slice+0.5f)*sliceDelta, 1, 1);

			// Front faces:
			verts.Add (vert1);
			verts.Add (vert2);
			verts.Add (vert3);
			verts.Add (vert4);
			normals.Add (-normal);
			normals.Add (-normal);
			normals.Add (-normal);
			normals.Add (-normal);
			// 1
			tris.Add (vertsAlreadyAdded + slice*4 + 0);
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
			tris.Add (vertsAlreadyAdded + slice*4 + 2);
			// 2
			tris.Add (vertsAlreadyAdded + slice*4 + 1);
			tris.Add (vertsAlreadyAdded + slice*4 + 3);
			tris.Add (vertsAlreadyAdded + slice*4 + 2);
		}

		// Generate the mesh object.
		Mesh ret = new Mesh();
		ret.vertices = verts.ToArray();
		ret.triangles = tris.ToArray();
		ret.normals = normals.ToArray();
		ret.uv = uvs.ToArray();

		// Assign the mesh object and update it.
		ret.RecalculateBounds();
		//ret.RecalculateNormals();
		stackOfSlices.mesh = ret;

		float diff = Time.time - start_time;
		Debug.Log("Stack Of Slices was generated in " + diff + " seconds.");
		Debug.Log ("Number of vertices: " + verts.Count);
		Debug.Log ("Number of normals: " + normals.Count);
	}

	public void SetDicom( DICOM3D dicom )
	{
		Material mat = GetComponent<MeshRenderer> ().sharedMaterial;
		Debug.Log ("Min, max: "+  (float)dicom.seriesInfo.minPixelValue + " " +  (float)dicom.seriesInfo.maxPixelValue);
		mat.SetFloat ("globalMinimum", (float)dicom.seriesInfo.minPixelValue);
		mat.SetFloat ("globalMaximum", (float)dicom.seriesInfo.maxPixelValue);
		mat.mainTexture = dicom.getTexture3D();

		/*float width = (float)dicom.getTexture3D ().width*(float)dicom.seriesInfo.pixelSpacing.x;
		float height = (float)dicom.getTexture3D ().height*(float)dicom.seriesInfo.pixelSpacing.y;
		float depth = (float)dicom.getTexture3D ().depth*(float)dicom.seriesInfo.sliceOffset.z;

		Debug.Log (width + " " + height + " " + depth);

		if (width == height && width == depth) {
			transform.localScale = new Vector3 (1f, 1f, 1f);
		} else if (width >= height && width >= depth) {
			transform.localScale = new Vector3 (
				1f,
				width/height,
				width/depth
			);
		} else if (height >= width && height >= depth) {
			transform.localScale = new Vector3 (
				height/width,
				height/depth,
				1f
			);
		} else {
			transform.localScale = new Vector3 (
				depth/width,
				1f,
				depth/height
			);
		}*/

		// Debug: Draw small sphere at origin:
		//Vector3 origin = dicom.seriesInfo.origin;
		//Debug.Log ("Origin: " + origin);	

		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.SetParent ( transform.parent );

		sphere.transform.localPosition = dicom.seriesInfo.boundingBox.min;

		// Corner points of the bounding box:
		Vector3 min = dicom.seriesInfo.boundingBox.min;
		Vector3 max = dicom.seriesInfo.boundingBox.max;
		Vector3 goalSize = dicom.seriesInfo.boundingBox.size;
		Vector3 paddingFactor = new Vector3 (
			(float)dicom.texWidth / (float)dicom.origTexWidth,
			(float)dicom.texHeight / (float)dicom.origTexHeight,
			(float)dicom.texDepth / (float)dicom.origTexDepth);
		Vector3 scale = Vector3.Scale( goalSize, paddingFactor )/ 2f;
		transform.localScale = scale;
		transform.localPosition = dicom.seriesInfo.boundingBox.center
		+ Vector3.Scale (dicom.seriesInfo.boundingBox.extents, paddingFactor - new Vector3 (1, 1, 1));
		Debug.Log ("paddingFactor " + paddingFactor);
		Debug.Log ("scale " + scale);
		Vector3 minMesh = new Vector3 (-1, -1, -1);
		Vector3 maxMesh = new Vector3 (1, 1, 1);

	}


	void eventDisplayCurrentDicom( object obj = null )
	{
		DICOM3D dicom = DICOMLoader.instance.currentDICOMVolume;
		if (dicom != null) {
			SetDicom (dicom);
			//GetComponent<MeshRenderer> ().enabled = true;
		}
	}

	void eventClear( object obj = null )
	{
		//GetComponent<MeshRenderer> ().enabled = false;
	}
}
