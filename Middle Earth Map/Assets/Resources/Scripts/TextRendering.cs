using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextRendering
{
    public class FontReader : IDisposable
    {
        public readonly Stream stream;
        public readonly BinaryReader reader;

        public FontReader(string Path)
        {
            stream = File.Open(Path, FileMode.Open);
            reader = new BinaryReader(stream);
        }

        public UInt16 ReadUInt16()
        {
            UInt16 value = reader.ReadUInt16();

            if (BitConverter.IsLittleEndian)
                value = (UInt16)(value >> 8 | value << 8);

            return value;
        }

        public void Dispose() { }
        public void SkipBytes(int num) => stream.Position += num;
    }

    public static void ParseFont(string Path)
    {
        FontReader fontReader = new FontReader(Path);

        fontReader.reader.BaseStream.Position += 4;
        UInt16 numTables = fontReader.reader.ReadUInt16();

        Debug.Log("Number of Tables: " + numTables); ;
    }
}
