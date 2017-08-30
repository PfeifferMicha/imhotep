using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlenderMeshReader
{
    //This class repesents a mesh in unity
    //Coordinate system is right-handed
    class BlenderMesh : MeshInterface
    {
        public string Name { get; set; }
        public Vector3[] VertexList { get; set; }
        public Vector3[] NormalList { get; set; }
        public List<PolygonListEntry> PolygonList { get; set; }
        public int[] LoopList { get; set; }
        public int[] TriangleList { get; set; }

        public BlenderMesh()
        {
            this.Name = "defaultMesh";
            VertexList = new Vector3[0];
            NormalList = new Vector3[0];
            PolygonList = new List<PolygonListEntry>();
            LoopList = new int[0];
            TriangleList = new int[0];
        }

        public BlenderMesh(string name)
        {
            this.Name = name;
            VertexList = new Vector3[0];
            NormalList = new Vector3[0];
            PolygonList = new List<PolygonListEntry>();
            LoopList = new int[0];
            TriangleList = new int[0];
        }

        //This method creates the TriangleList from PolygonList and LoopList.
        //This method must be called after the VertexList, PolygonList and LoopList are complete
        public void createTriangleList()
        {
            List<int> triangle = new List<int>(); //TODO check what is faster: List (create List, add elements, ToArray()) or Array (calc length, fill array)
            foreach (PolygonListEntry polygon in PolygonList)
            {
                for (int i = 0; i < polygon.Lenght - 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (i == 0)
                        {
                            triangle.Add(LoopList[polygon.StartIndex + j]);
                        }
                        else
                        {
                            if (j != 2)
                            {
                                triangle.Add(LoopList[polygon.StartIndex + j + i + 1]);
                            }
                            else
                            {
                                triangle.Add(LoopList[polygon.StartIndex]);
                            }
                        }

                    }

                }
            }
            TriangleList = triangle.ToArray();


            return;
        }

        //Return a unity mesh in a left-handed coordinate system
        public UnityMesh ToUnityMesh()
        {
            UnityMesh result = new UnityMesh(this.Name);

            Vector3[] vertices = new Vector3[VertexList.Length];
            //Flip z component of all Vector3 in vertex list
            for (int i = 0; i < VertexList.Length; i++)
            {
                vertices[i] = new Vector3(VertexList[i].x, VertexList[i].y,-VertexList[i].z);
            }
            result.VertexList = vertices;

            Vector3[] normals = new Vector3[NormalList.Length];
            //Flip z component of all Vector3 in normal list
            for (int i = 0; i < NormalList.Length; i++)
            {
                normals[i] = new Vector3(NormalList[i].x, NormalList[i].y, -NormalList[i].z);
            }
            result.NormalList = normals;

            int[] triangles = new int[TriangleList.Length];
            //Flip the order of triangle vertices. v0,v1,v2 -> v0,v2,v1.
            for (int i = 0; i < TriangleList.Length; i += 3)
            {
                triangles[i] = TriangleList[i];
                triangles[i + 1] = TriangleList[i + 2];
                triangles[i + 2] = TriangleList[i + 1];
            }
            result.TriangleList = triangles;

            return result;

        }


    }
}
