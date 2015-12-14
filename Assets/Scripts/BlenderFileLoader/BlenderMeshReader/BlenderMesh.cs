using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlenderMeshReader
{

    class BlenderMesh
    {
        //X,Y,Z coordinates of vertices
        public Vector3[] VertexList { get; set; }
        public Vector3[] NormalList { get; set; }
        //List of Polygons. First value is start index in LoopList, second value is length of polygon (in general 3)
        public List<PolygonListEntry> PolygonList { get; set; }
        //List of Loops. The value is index in VertexList
        public int[] LoopList { get; set; }


        public BlenderMesh()
        {
            VertexList = new Vector3[0];
            PolygonList = new List<PolygonListEntry>();
            LoopList = new int[0];
        }
    }
}
