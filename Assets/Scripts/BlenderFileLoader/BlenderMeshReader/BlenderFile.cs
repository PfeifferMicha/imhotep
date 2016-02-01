using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BlenderMeshReader
{
    [StructLayout(LayoutKind.Explicit, Pack = 4, Size = 8)]
    struct MLoop
    {
        [FieldOffset(0)]
        public int v;
        [FieldOffset(4)]
        public int e;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
    struct MPoly
    {
        [FieldOffset(0)]
        public int loopstart;
        [FieldOffset(4)]
        public int totloop;
        [FieldOffset(8)]
        public short mat_nr;
        [FieldOffset(10)]
        public byte flag;
        [FieldOffset(11)]
        public byte pad;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 20)]
    struct MVert
    {
        [FieldOffset(0)]
        public float coX;
        [FieldOffset(4)]
        public float coY;
        [FieldOffset(8)]
        public float coZ;

        [FieldOffset(12)]
        public short noX;
        [FieldOffset(14)]
        public short noY;
        [FieldOffset(16)]
        public short noZ;

        [FieldOffset(18)]
        public byte flag;

        [FieldOffset(19)]
        public byte bweight;
    }

    class BlenderFile
    {
        public static int PointerSize { get; private set; }
        public string BlenderVersionNumber { get; private set; }
        public string Filename { get; private set; }
        public List<FileBlock> FileBockList { get; private set; }
        public List<Structure> StructureList { get; private set; }


        //private BinaryReader reader;

        public BlenderFile(string path)
        {
            StructureList = new List<Structure>();
            Filename = path;
            BlenderVersionNumber = null;
            readHeader();
            FileBockList = readFileBlockList();
            foreach(FileBlock f in FileBockList)
            {
                if(f.Code == "DNA1")
                {
                    StructureList = readDNA1Block(f.StartAddess, f.Size);
                }
            }
            //reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read));
        }

        private void readHeader()
        {
            if (!File.Exists(Filename))
            {
                throw new FileNotFoundException("File not found.");
            }

            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open ));

            if(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(7)) != "BLENDER")
            {
                throw new FormatException("Wrong file format.");
            }
            PointerSize = Convert.ToChar(reader.ReadByte()) == '-' ? 8 : 4; // '-' = 8, '_' = 4
            char end = Convert.ToChar(reader.ReadByte()); // 'V' = big endian, 'v' = little endian
            if ((end != 'v' && end != 'V') || (end == 'v' && !BitConverter.IsLittleEndian) || (end == 'V' && BitConverter.IsLittleEndian))
            {
                throw new FormatException("Endianness of computer does not match with endianness of file. Open the file in Blender and save it to convert.");
            }
            BlenderVersionNumber = new string(new[] { Convert.ToChar(reader.ReadByte()), '.', Convert.ToChar(reader.ReadByte()),  Convert.ToChar(reader.ReadByte()) });

            reader.Close();      

        }

        private List<FileBlock> readFileBlockList()
        {
            List<FileBlock> result = new List<FileBlock>();
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open));

            reader.ReadBytes(12); //skip file header

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                // read block header
                long startaddess = reader.BaseStream.Position;
                string code = new string(reader.ReadChars(4));
                int size = reader.ReadInt32();
                ulong oldaddress = PointerSize == 8 ? reader.ReadUInt64() : reader.ReadUInt32(); //not used
                int sdna = reader.ReadInt32();
                int count = reader.ReadInt32();

                FileBlock f = new FileBlock(code, size, sdna, count, startaddess, oldaddress);
                result.Add(f);

                reader.BaseStream.Position = reader.BaseStream.Position + size;
                //reader.ReadBytes(size); //skip data

            }

            reader.Close();
            return result;

        }


        private List<Structure> readDNA1Block(long startAddress, int size)
        {
            //List<FileBlock> result = new List<FileBlock>();
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open));
            reader.BaseStream.Position = startAddress + (PointerSize == 8 ? 24 : 20) + 4; //set position to data

            reader.ReadBytes(4); //read NAME
            int numberOfNames = reader.ReadInt32();
            string[] nameArray = new string[numberOfNames];
            for(int i = 0; i<numberOfNames; i++)
            {
                string name = "";
                char c;
                while((c = reader.ReadChar()) != 0x0)
                {
                    name += c;
                }
                nameArray[i] = name;
            }
            //next date is aligned at four bytes
            while (reader.BaseStream.Position % 4 != 0)
            {
                reader.BaseStream.Position++;
            }


            reader.ReadBytes(4); //read TYPE
            int numberOfTypes = reader.ReadInt32();
            Type[] typeArray = new Type[numberOfTypes];
            for (int i = 0; i < numberOfTypes; i++)
            {
                string name = "";
                char c;
                while ((c = reader.ReadChar()) != 0x0)
                {
                    name += c;
                }
                typeArray[i] = new Type(name);
            }
            //next date is aligned at four bytes
            while (reader.BaseStream.Position % 4 != 0)
            {
                reader.BaseStream.Position++;
            }


            reader.ReadBytes(4); //read TLEN
            for (int i = 0; i < numberOfTypes; i++)
            {
                short s = reader.ReadInt16();
                typeArray[i].Length = s;
            }
            //next date is aligned at four bytes
            while (reader.BaseStream.Position % 4 != 0)
            {
                reader.BaseStream.Position++;
            }


            reader.ReadBytes(4); //read STRC
            List<Structure> sList = new List<Structure>();
            int numberOfStructure = reader.ReadInt32();
            for (int i = 0; i < numberOfStructure; i++)
            {
                short nameIndex = reader.ReadInt16();
                Structure st = new Structure(typeArray[nameIndex].Name, i);
                sList.Add(st);
                short numberOfFields = reader.ReadInt16();
                for(short s = 0; s< numberOfFields; s++)
                {
                    short indexType = reader.ReadInt16();
                    short indexName = reader.ReadInt16();
                    Field f = new Field(nameArray[indexName], typeArray[indexType]);
                    //Field f = new Field(nameArray[indexName], new Type(typeArray[indexType].Name, typeArray[indexType].Length));
                    st.Fields.Add(f);
                }
            }

            reader.Close();
            return sList;
        }

        public List<BlenderMesh> readMesh()
        {
            List<BlenderMesh> result = new List<BlenderMesh>();

            //get information about the structure of a mesh block
            int indexMesh = 0;

            int startPositionMVert = -1;
            int lengthMVert = 0;

            int startPositionMPoly = -1;
            int lengthMPoly = 0;

            int startPositionMLoop = -1;
            int lengthMLoop = 0;

            foreach (Structure s in StructureList)
            {
                if(s.Name == "Mesh") //search index of mesh structure and start position of *mvert, *mpoly, *mloop in mesh structure
                {
                    indexMesh = s.Index;
                    int countLenght = 0;
                    foreach(Field f in s.Fields)
                    {
                        if (f.Name == "*mvert")
                        {
                            startPositionMVert = countLenght;
                        }
                        else if (f.Name == "*mpoly")
                        {
                            startPositionMPoly = countLenght;
                        }
                        else if (f.Name == "*mloop")
                        {
                            startPositionMLoop = countLenght;
                        }
                        countLenght += f.getLength();
                    }
                }
                
                if(s.Name == "MVert") //search for structure information of MVert
                {
                    lengthMVert = s.getLength();
                }

                if (s.Name == "MPoly") //search for structure information of MPoly
                {
                    lengthMPoly = s.getLength();
                }

                if (s.Name == "MLoop") //search for structure information of MLoop
                {
                    lengthMLoop = s.getLength();
                }

            }
            //read vertices, polys and loops
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open));
            foreach (FileBlock fileBlock in FileBockList)
            {
                if(fileBlock.SDNAIndex == indexMesh)
                {
                    //read name
                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + 32;
                    string name = "";
                    for(int i = 0; i < 66; i++)
                    {
                        char c = reader.ReadChar();
                        if(c == 0x0)
                        {
                            break;
                        }
                        name += c;
                    }

                    BlenderMesh currentMesh = new BlenderMesh(name);
                    result.Add(currentMesh);

                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + startPositionMVert;
                    ulong mVertAddress = PointerSize == 8 ? reader.ReadUInt64() : reader.ReadUInt32(); //pointer to file block with MVert structures

                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + startPositionMPoly;
                    ulong mPolyAddress = PointerSize == 8 ? reader.ReadUInt64() : reader.ReadUInt32(); //pointer to file block with MEdge structure

                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + startPositionMLoop;
                    ulong mLoopAddress = PointerSize == 8 ? reader.ReadUInt64() : reader.ReadUInt32(); //pointer to file block with MEdge structure

                    foreach (FileBlock f in FileBockList)
                    {
                        //Read vertices and normals
                        if (f.OldAddess == mVertAddress)
                        {
                            currentMesh.VertexList = new Vector3[f.Count];
                            currentMesh.NormalList = new Vector3[f.Count];

                            MVert[] readVerts = new MVert[f.Count];
                            reader.BaseStream.Position = f.StartAddess + (PointerSize == 8 ? 24 : 20);
                            byte[] readBytes = reader.ReadBytes(f.Count * Marshal.SizeOf(typeof(MVert)));
                            GCHandle pinnedHandle = GCHandle.Alloc(readVerts, GCHandleType.Pinned);
                            Marshal.Copy(readBytes, 0, pinnedHandle.AddrOfPinnedObject(), readBytes.Length);
                            pinnedHandle.Free();

                            for (int i = 0; i < readVerts.Length; i++)
                            {
                                currentMesh.VertexList[i] = new Vector3(readVerts[i].coX, readVerts[i].coY, readVerts[i].coZ);
                                currentMesh.NormalList[i] = new Vector3(readVerts[i].noX, readVerts[i].noY, readVerts[i].noZ);
                            }
                        }
                        //Read polygon list
                        else if (f.OldAddess == mPolyAddress)
                        {
                            MPoly[] readPoly = new MPoly[f.Count];
                            reader.BaseStream.Position = f.StartAddess + (PointerSize == 8 ? 24 : 20);
                            byte[] readBytes = reader.ReadBytes(f.Count * Marshal.SizeOf(typeof(MPoly)));
                            GCHandle pinnedHandle = GCHandle.Alloc(readPoly, GCHandleType.Pinned);
                            Marshal.Copy(readBytes, 0, pinnedHandle.AddrOfPinnedObject(), readBytes.Length);
                            pinnedHandle.Free();
                            
                            for (int i = 0; i < readPoly.Length; i++)
                            {
                               currentMesh.PolygonList.Add(new PolygonListEntry(readPoly[i].loopstart, readPoly[i].totloop));
                            }
                        }
                        //Read loop list
                        else if (f.OldAddess == mLoopAddress)
                        {
                            currentMesh.LoopList = new int[f.Count];

                            reader.BaseStream.Position = f.StartAddess + (PointerSize == 8 ? 24 : 20);
                            MLoop[] readLoop = new MLoop[f.Count];
                            byte[] readBytes = reader.ReadBytes(f.Count * Marshal.SizeOf(typeof(MLoop)));
                            GCHandle pinnedHandle = GCHandle.Alloc(readLoop, GCHandleType.Pinned);
                            Marshal.Copy(readBytes, 0, pinnedHandle.AddrOfPinnedObject(), readBytes.Length);
                            pinnedHandle.Free();

                            for(int i = 0; i< readLoop.Length; i++)
                            {
                                currentMesh.LoopList[i] = readLoop[i].v;
                            }
                        }
                    }
                    currentMesh.createTriangleList();


                }
            }

            /*foreach(BlenderMesh m in result)
            {
                foreach (Vector3 t in m.VertexList)
                {
                    Debug.Log(t);
                }

                foreach (PolygonListEntry t in m.PolygonList)
                {
                    Debug.Log(t);
                }

                foreach (int t in m.LoopList)
                {
                    Debug.Log(t);
                }
            }*/

            reader.Close();
            return result;
        }

        //Creates a list of lists with BlenderMeshes. The inner list contains multipe BlenderMeshes for one object with max. 2^16 vertices, because of Unitys limitation.
        public static List<List<UnityMesh>> createSubmeshesForUnity(List<BlenderMesh> blenderMesh)
        {
            List<List<UnityMesh>> result = new List<List<UnityMesh>>();

            //Iterate all complete meshes found in file
            foreach (BlenderMesh completeBlenderMesh in blenderMesh)
            {

                UnityMesh completeMesh = completeBlenderMesh.ToUnityMesh();

                List<UnityMesh> outterListElement = new List<UnityMesh>();
                result.Add(outterListElement);

                int maxVerts = 6000;//65534; //TODO this is only a workaround 
                int positionTriangleList = 0;
                //Go over the complete triangle list of mesh
                while (positionTriangleList < completeMesh.TriangleList.Length)
                {
                    UnityMesh newMesh = new UnityMesh(completeMesh.Name); //Create submesh
                    outterListElement.Add(newMesh);
                    List<Vector3> vertList = new List<Vector3>(); //Create vertices list for submesh
                    List<Vector3> normList = new List<Vector3>(); //Create normal list for submesh
                    List<int> triangles = new List<int>(); //Create triangle list for submesh

                    int[] findVertexIndex = new int[completeMesh.VertexList.Length]; //for optimization, key: vertex index of complete mesh, value: vertex index of new mesh
                    for(int i = 0; i < findVertexIndex.Length; i++) //memset() faster?
                    {
                        findVertexIndex[i] = -1;
                    }

                    //Write vertex, normal and triangle infos in variables until maxVert is reached or end of TriangleList is reached
                    while (vertList.Count < maxVerts - 3 && positionTriangleList < completeMesh.TriangleList.Length)
                    {
                        for(int i = 0; i < 3; i++)
                        {
                            int indexCurrentVertex = completeMesh.TriangleList[positionTriangleList];
                            int newIndex = findVertexIndex[indexCurrentVertex];

                            if (newIndex == -1)
                            {
                                vertList.Add(completeMesh.VertexList[indexCurrentVertex]);
                                normList.Add(completeMesh.NormalList[indexCurrentVertex]);
                                findVertexIndex[indexCurrentVertex] = vertList.Count - 1;
                                triangles.Add(vertList.Count - 1);
                            }
                            else
                            {
                                triangles.Add(newIndex);
                            }
                            positionTriangleList++;
                        }
                    }
                    newMesh.VertexList = vertList.ToArray();
                    newMesh.NormalList = normList.ToArray();
                    newMesh.TriangleList = triangles.ToArray();
                }

            }
            return result;
        }
    }
}

