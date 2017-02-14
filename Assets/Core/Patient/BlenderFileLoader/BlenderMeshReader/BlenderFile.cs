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


    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 104)]
    struct BoundBox
    {
        [FieldOffset(0)]
        public float vec00;
        [FieldOffset(4)]
        public float vec10;
        [FieldOffset(8)]
        public float vec20;
        [FieldOffset(12)]
        public float vec30;
        [FieldOffset(16)]
        public float vec40;
        [FieldOffset(20)]
        public float vec50;
        [FieldOffset(24)]
        public float vec60;
        [FieldOffset(28)]
        public float vec70;

        [FieldOffset(32)]
        public float vec01;
        [FieldOffset(36)]
        public float vec11;
        [FieldOffset(40)]
        public float vec21;
        [FieldOffset(44)]
        public float vec31;
        [FieldOffset(48)]
        public float vec41;
        [FieldOffset(52)]
        public float vec51;
        [FieldOffset(56)]
        public float vec61;
        [FieldOffset(60)]
        public float vec71;

        [FieldOffset(64)]
        public float vec02;
        [FieldOffset(68)]
        public float vec12;
        [FieldOffset(72)]
        public float vec22;
        [FieldOffset(76)]
        public float vec32;
        [FieldOffset(80)]
        public float vec42;
        [FieldOffset(84)]
        public float vec52;
        [FieldOffset(88)]
        public float vec62;
        [FieldOffset(92)]
        public float vec72;

        [FieldOffset(96)]
        public int flag;

        [FieldOffset(100)]
        public int pad;
    }

    struct BlenderObjectBlock
    {
        public string objectName;
        public Vector3 location;
        public Quaternion rotation;
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

        //Return a list of bounding boxes or an empty list if there are no bounding boxes 
        public List<BoundingBox> readBoundingBoxes()
        {
            List<BoundingBox> result = new List<BoundingBox>();

            //get information about the structure of a mesh block
            int indexMesh = 0;

            int startPositionBoundBox = -1;
   

            foreach (Structure s in StructureList)
            {
                if (s.Name == "Mesh") //search index of mesh structure and start position of *bb in mesh structure
                {
                    indexMesh = s.Index;
                    int countLenght = 0;
                    foreach (Field f in s.Fields)
                    {
                        if (f.Name == "*bb")
                        {
                            startPositionBoundBox = countLenght;
                        }
                        countLenght += f.getLength();
                    }
                }

            }
            //read bound box
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open));
            foreach (FileBlock fileBlock in FileBockList)
            {
                if (fileBlock.SDNAIndex == indexMesh)
                {
                    //read name
                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + 32;
                    string name = "";
                    for (int i = 0; i < 66; i++)
                    {
                        char c = reader.ReadChar();
                        if (c == 0x0)
                        {
                            break;
                        }
                        name += c;
                    }
                    
                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + startPositionBoundBox;
                    ulong boundBoxAddress = PointerSize == 8 ? reader.ReadUInt64() : reader.ReadUInt32(); //pointer to file block with BoundBox structures
                    if(boundBoxAddress == 0)
                    {
                        continue; //continue if there are no bounding box
                    }

                    BoundingBox currentBoundingBox = new BoundingBox();
                    currentBoundingBox.name = name;
                    result.Add(currentBoundingBox);

                    foreach (FileBlock f in FileBockList)
                    {
                        //Read bound box
                        if (f.OldAddess == boundBoxAddress)
                        {

                            BoundBox readBoundBox = new BoundBox();
                            reader.BaseStream.Position = f.StartAddess + (PointerSize == 8 ? 24 : 20);
                            byte[] readBytes = reader.ReadBytes(f.Count * Marshal.SizeOf(typeof(BoundBox)));
                            GCHandle pinnedHandle = GCHandle.Alloc(readBoundBox, GCHandleType.Pinned);
                            Marshal.Copy(readBytes, 0, pinnedHandle.AddrOfPinnedObject(), readBytes.Length);
                            pinnedHandle.Free();

                            currentBoundingBox.vec[0, 0] = readBoundBox.vec00;
                            currentBoundingBox.vec[1, 0] = readBoundBox.vec10;
                            currentBoundingBox.vec[2, 0] = readBoundBox.vec20;
                            currentBoundingBox.vec[3, 0] = readBoundBox.vec30;
                            currentBoundingBox.vec[4, 0] = readBoundBox.vec40;
                            currentBoundingBox.vec[5, 0] = readBoundBox.vec50;
                            currentBoundingBox.vec[6, 0] = readBoundBox.vec60;
                            currentBoundingBox.vec[7, 0] = readBoundBox.vec70;
                            currentBoundingBox.vec[0, 1] = readBoundBox.vec01;
                            currentBoundingBox.vec[1, 1] = readBoundBox.vec11;
                            currentBoundingBox.vec[2, 1] = readBoundBox.vec21;
                            currentBoundingBox.vec[3, 1] = readBoundBox.vec31;
                            currentBoundingBox.vec[4, 1] = readBoundBox.vec41;
                            currentBoundingBox.vec[5, 1] = readBoundBox.vec51;
                            currentBoundingBox.vec[6, 1] = readBoundBox.vec61;
                            currentBoundingBox.vec[7, 1] = readBoundBox.vec71;
                            currentBoundingBox.vec[0, 2] = readBoundBox.vec02;
                            currentBoundingBox.vec[1, 2] = readBoundBox.vec12;
                            currentBoundingBox.vec[2, 2] = readBoundBox.vec22;
                            currentBoundingBox.vec[3, 2] = readBoundBox.vec32;
                            currentBoundingBox.vec[4, 2] = readBoundBox.vec42;
                            currentBoundingBox.vec[5, 2] = readBoundBox.vec52;
                            currentBoundingBox.vec[6, 2] = readBoundBox.vec62;
                            currentBoundingBox.vec[7, 2] = readBoundBox.vec72;

                        }
                       
                    }
                }
            }

            reader.Close();
            return result;
        }

        public List<BlenderObjectBlock> readObject()
        {
            List<BlenderObjectBlock> result = new List<BlenderObjectBlock>();
            
            int objIndex = 0;
            int startPositionLoc = 0;
            int startPositinQuat = 0;

            foreach (Structure s in StructureList)
            {
                if (s.Name == "Object") //search index of object structure 
                {
                    objIndex = s.Index;
                    int countLenght = 0;
                    foreach (Field f in s.Fields)
                    {
                        if (f.Name == "loc[3]")
                        {
                            startPositionLoc = countLenght;
                        }
                        else if (f.Name == "quat[4]")
                        {
                            startPositinQuat = countLenght;
                        }
                        countLenght += f.getLength();
                    }
                }
                
            }
            
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open));            
            foreach (FileBlock fileBlock in FileBockList)
            {
                if (fileBlock.SDNAIndex == objIndex)
                {
                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + 32;
                    string name = "";
                    for (int i = 0; i < 66; i++)
                    {
                        char c = reader.ReadChar();
                        if (c == 0x0)
                        {
                            break;
                        }
                        name += c;
                    }
                    
                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + startPositionLoc;
                    float[] loc = new float[3];
                    loc[0] = reader.ReadSingle();
                    loc[1] = reader.ReadSingle();
                    loc[2] = reader.ReadSingle();

                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + startPositinQuat;
                    float[] quat = new float[4];
                    quat[0] = reader.ReadSingle();
                    quat[1] = reader.ReadSingle();
                    quat[2] = reader.ReadSingle();
                    quat[3] = reader.ReadSingle();


                    BlenderObjectBlock b = new BlenderObjectBlock();
                    b.objectName = name;
                    b.location = new Vector3(loc[0], loc[1], loc[2]);
                    b.rotation = new Quaternion(quat[0], quat[1], quat[2], quat[3]);
                    result.Add(b);
                }

            }
            reader.Close();
            return result;
        }

        public List<BlenderMesh> readMesh()
        {
            List<BlenderMesh> result = new List<BlenderMesh>();

            //get information about the structure of a mesh block
            int indexMesh = 0;

            int startPositionMVert = -1;
            //int lengthMVert = 0;

            int startPositionMPoly = -1;
            //int lengthMPoly = 0;

            int startPositionMLoop = -1;
            //int lengthMLoop = 0;

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
                
                /*if(s.Name == "MVert") //search for structure information of MVert
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
                }*/

            }
            //read vertices, polys and loops
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open));
            foreach (FileBlock fileBlock in FileBockList)
            {
                if(fileBlock.SDNAIndex == indexMesh)
                {
                    //read name
                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + 32; //32 because name has an offset of 32 in the ID structure 
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

