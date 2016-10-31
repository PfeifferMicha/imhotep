using System;

namespace BlenderMeshReader
{
    class FileBlock
    {
        //blender file doc from http://www.atmind.nl/blender/mystery_ot_blend.html
        public string Code { get; private set; } //Identifier of the file-block
        public int Size { get; private set; } //Total length of the data after the file-block-header
        public int SDNAIndex { get; private set; } //Index of the SDNA structure
        public int Count { get; private set; } //Number of structure located in this file-block
        public long StartAddess { get; private set; } //Start address of the blog
        public ulong OldAddess { get; private set; } //Start address of the blog


        public FileBlock(string code, int size, int sDNAIndex, int count, long starAddress, ulong oldAdress)
        {
            this.Code = code;
            this.Size = size;
            this.SDNAIndex = sDNAIndex;
            this.Count = count;
            this.StartAddess = starAddress;
            this.OldAddess = oldAdress;
        }

        public override string ToString()
        {
            return "File-block:" + Environment.NewLine + "Code: " + Code + 
                Environment.NewLine + "Size: " + Size + 
                Environment.NewLine + "SDNA index: " + SDNAIndex + 
                Environment.NewLine + "Count: " + Count + 
                Environment.NewLine + "Start address: " + StartAddess +
                Environment.NewLine + "Old address: " + OldAddess +
                Environment.NewLine;
        }

    }
}
