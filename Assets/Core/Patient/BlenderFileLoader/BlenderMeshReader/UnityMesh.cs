using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlenderMeshReader
{
    //This class repesents a mesh in unity
    //Coordinate system is left-handed
    class UnityMesh : MeshInterface
    {
        public string Name { get; set; }
        public ulong UniqueIdentifier { get; set; }
        public Vector3[] VertexList { get; set; }
        public Vector3[] NormalList { get; set; }
        public int[] TriangleList { get; set; }

        public UnityMesh()
        {
            this.Name = "defaultMesh";
            this.UniqueIdentifier = 0;
            VertexList = new Vector3[0];
            NormalList = new Vector3[0];
            TriangleList = new int[0];
        }

        public UnityMesh(string name, ulong uniqueIdentifier)
        {
			// Remove the "ME" at the beginning of the file name:
			if (name.Substring (0, 2) == "ME") {
				name = name.Substring (2, name.Length - 2);
			}
			this.Name = name;
            this.UniqueIdentifier = uniqueIdentifier;
            VertexList = new Vector3[0];
            NormalList = new Vector3[0];
            TriangleList = new int[0];
        }
    }
}
