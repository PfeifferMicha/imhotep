﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BlenderMeshReader
{
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

            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open, FileAccess.Read));

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
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open, FileAccess.Read));

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
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open, FileAccess.Read));
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

        public List<BlenderMesh> readMeshVertices()
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
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open, FileAccess.Read));
            foreach (FileBlock fileBlock in FileBockList)
            {
                if(fileBlock.SDNAIndex == indexMesh)
                {
                    BlenderMesh currentMesh = new BlenderMesh();
                    result.Add(currentMesh);

                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + startPositionMVert;
                    ulong mVertAddress = PointerSize == 8 ? reader.ReadUInt64() : reader.ReadUInt32(); //pointer to file block with MVert structures

                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + startPositionMPoly;
                    ulong mPolyAddress = PointerSize == 8 ? reader.ReadUInt64() : reader.ReadUInt32(); //pointer to file block with MEdge structure

                    reader.BaseStream.Position = fileBlock.StartAddess + (PointerSize == 8 ? 24 : 20) + startPositionMLoop;
                    ulong mLoopAddress = PointerSize == 8 ? reader.ReadUInt64() : reader.ReadUInt32(); //pointer to file block with MEdge structure

                    foreach (FileBlock f in FileBockList)
                    {
                        if(f.OldAddess == mVertAddress)
                        {
                            currentMesh.VertexList = new Vector3[f.Count];
                            currentMesh.NormalList = new Vector3[f.Count];

                            reader.BaseStream.Position = f.StartAddess + (PointerSize == 8 ? 24 : 20);
                            for (int i = 0; i < f.Count; i++)
                            {
                                float vertX = reader.ReadSingle();
                                float vertY = reader.ReadSingle();
                                float vertZ = reader.ReadSingle();
                                currentMesh.VertexList[i] = new Vector3(vertX, vertY, vertZ);
                                short noX = reader.ReadInt16();
                                short noY = reader.ReadInt16();
                                short noZ = reader.ReadInt16(); ;
                                currentMesh.NormalList[i] = new Vector3(noX, noY, noZ);
                                reader.BaseStream.Position += lengthMVert - 18 ; //skip other data in MVert (co[3] + no[3] = 18byte)
                            }
                        }

                        if (f.OldAddess == mPolyAddress)
                        {
                            reader.BaseStream.Position = f.StartAddess + (PointerSize == 8 ? 24 : 20);
                            for (int i = 0; i < f.Count; i++)
                            {
                                int loopstart = reader.ReadInt32();
                                int totloop = reader.ReadInt32();
                                currentMesh.PolygonList.Add(new PolygonListEntry(loopstart, totloop));
                                reader.BaseStream.Position += lengthMPoly - 8; //skip other data in MPoly (loopstart + totloop = 8byte)
                            }
                        }

                        if (f.OldAddess == mLoopAddress)
                        {
                            currentMesh.LoopList = new int[f.Count];

                            reader.BaseStream.Position = f.StartAddess + (PointerSize == 8 ? 24 : 20);
                            for (int i = 0; i < f.Count; i++)
                            {
                                currentMesh.LoopList[i] = reader.ReadInt32();
                                reader.BaseStream.Position += lengthMLoop - 4; //skip other data in MEdge (v  = 4byte)
                            }
                        }


                    }

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

    }
}

