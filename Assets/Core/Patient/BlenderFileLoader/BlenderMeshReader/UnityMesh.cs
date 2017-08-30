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
        public Vector3[] VertexList { get; set; }
        public Vector3[] NormalList { get; set; }
        public int[] TriangleList { get; set; }

        public UnityMesh()
        {
            this.Name = "defaultMesh";
            VertexList = new Vector3[0];
            NormalList = new Vector3[0];
            TriangleList = new int[0];
        }

        public UnityMesh(string name)
        {
            this.Name = name;
            VertexList = new Vector3[0];
            NormalList = new Vector3[0];
            TriangleList = new int[0];
        }
    }
}
