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


		// ------------------------------------------------
		// Move the VolumeCube to the position where the 3D DICOM should be rendered:
		// (The VolumeCube's localPosition and localScale is in the DICOM's "patient coordinate system", so
		// adjusting its localPosition and localScale to fit with the DICOM's center and size will set it to
		// the correct position.)

		// Size which the volume should have at the end (in patient coordinates):
		Vector3 goalSize = dicom.seriesInfo.boundingBox.size;
		// Position where the center of the rendered volume should be (in patient coordinates):
		Vector3 goalCenter = dicom.seriesInfo.boundingBox.center;
		// Current (original) position and sizes of the mesh, because of the way it was generated:
		Vector3 minMesh = new Vector3 (-1, -1, -1);
		Vector3 maxMesh = new Vector3 (1, 1, 1);
		Vector3 meshSize = maxMesh - minMesh;
		// The padding factor takes into account that the DICOM volume may be smaller than the texture, since
		// Unity requires power-of-two textures, so the texture may be larger than the DICOM volume.
		Vector3 paddingFactor = new Vector3 (
			(float)dicom.origTexWidth / (float)dicom.texWidth,
			(float)dicom.origTexHeight / (float)dicom.texHeight,
			(float)dicom.origTexDepth / (float)dicom.texDepth);

		// Part of the mesh which actually holds the DICOM volume:
		Vector3 paddedMeshSize = Vector3.Scale (paddingFactor, meshSize);

		// Center of the part of the mesh which is filled with the DICOM volume:
		Vector3 meshCenter = minMesh + paddedMeshSize*0.5f;
		// Offset from the mesh original center (which is, because of the way the mesh was generated above, zero):
		Vector3 meshCenterOffset = Vector3.zero - meshCenter;

		// Inverse of paddedMeshSize:
		Vector3 invPaddedMeshSize = new Vector3 (1f / paddedMeshSize.x, 1f / paddedMeshSize.y, 1f / paddedMeshSize.z);
		// The final scale is the goalSize/paddedMeshSize:
		Vector3 scale = Vector3.Scale (goalSize, invPaddedMeshSize);
		transform.localScale = scale;
		// Move the mesh center to its final position:
		transform.localPosition = goalCenter + Vector3.Scale (meshCenterOffset, scale);

		// ------------------------------------------------
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
