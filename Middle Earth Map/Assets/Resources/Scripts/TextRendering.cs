using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextRendering
{
    public class FontReader : IDisposable
    {
        private readonly Stream stream;
        private readonly BinaryReader reader;

        public class GlyphData
        {
            private int[] xCoords;
            private int[] yCoords;
            private int[] contourEndIndices;

            public GlyphData(int[] x, int[] y, int[] EndPoints) => (xCoords, yCoords, contourEndIndices) = (x, y, EndPoints);
        }

        public FontReader(string Path)
        {
            stream = File.Open(Path, FileMode.Open);
            reader = new BinaryReader(stream);
        }

        public static GlyphData ReadSimpleGlyph(FontReader reader)
        {
            int[] contourEndIndices = new int[reader.ReadUInt16()];
            reader.SkipBytes(8);

            for (int i = 0; i < contourEndIndices.Length; i++)
                contourEndIndices[i] = reader.ReadUInt16();

            int numPoints = contourEndIndices[^1] + 1;
            byte[] allFlags = new byte[numPoints];
            reader.SkipBytes(reader.ReadUInt16());

            for (int i = 0; i < numPoints; i++)
            {
                byte flag = reader.ReadByte();
                allFlags[i] = flag;

                if (FlagBitIsSet(flag, 3))
                    for (int r = 0; r < reader.ReadByte(); r++)
                        allFlags[++i] = flag;
            }

            int[] xCoords = ReadCoordinates(reader, allFlags, true);
            int[] yCoords = ReadCoordinates(reader, allFlags, false);
            return new GlyphData(xCoords, yCoords, contourEndIndices);
        }

        static int[] ReadCoordinates(FontReader reader, byte[] allFlags, bool readingX)
        {
            int offsetSizeFlagBit = readingX ? 1 : 2;
            int offsetSignOrSkipBit = readingX ? 4 : 5;
            int[] coordinates = new int[allFlags.Length];

            for (int i = 0; i < coordinates.Length; i++)
            {
                coordinates[i] = coordinates[Math.Max(0, i - 1)];
                byte flag = allFlags[i];
                bool onCurve = FlagBitIsSet(flag, 0);

                if (FlagBitIsSet(flag, offsetSizeFlagBit))
                {
                    byte offset = reader.ReadByte();
                    int sign = FlagBitIsSet(flag, offsetSignOrSkipBit) ? 1 : -1;
                    coordinates[i] += offset * sign;
                }
                else if (!FlagBitIsSet(flag, offsetSignOrSkipBit))
                {
                    coordinates[i] += reader.ReadUInt16();
                }
            }

            return coordinates;
        }

        public UInt16 ReadUInt16()
        {
            UInt16 value = reader.ReadUInt16();

            if (BitConverter.IsLittleEndian)
                value = (UInt16)(value >> 8 | value << 8);

            return value;
        }

        public UInt32 ReadUInt32()
        {
            UInt32 value = reader.ReadUInt32();

            if (BitConverter.IsLittleEndian)
            {
                const byte mask = 0b111111;
                UInt32 a = (value >> 24) & mask;
                UInt32 b = (value >> 16) & mask;
                UInt32 c = (value >> 8) & mask;
                UInt32 d = (value >> 0) & mask;

                value = a << 0 | b << 8 | c << 16 | d << 24;
            }

            return value;
        }

        public string ReadTag()
        {
            Span<char> tag = stackalloc char[4];

            for (int i = 0; i < tag.Length; i++)
                tag[i] = (char)reader.ReadByte();

            return tag.ToString();
        }

        public void Dispose() { }

        public byte ReadByte() => reader.ReadByte();

        public void SkipBytes(int num) => stream.Position += num;

        public void GoTo(uint num) => stream.Position = num;

        static bool FlagBitIsSet(byte flag, int index) => ((flag >> index) & 1) == 1;
    }

    public static void ParseFont(string Path)
    {
        FontReader reader = new FontReader(Path);

        reader.SkipBytes(4);
        UInt16 numTables = reader.ReadUInt16();

        Debug.Log("Number of Tables: " + numTables);

        reader.SkipBytes(6);

        Dictionary<string, uint> tableLocationLookup = new Dictionary<string, uint>();

        for (int i = 0; i < numTables; i++)
        {
            string tag = reader.ReadTag();
            uint checksum = reader.ReadUInt32();
            uint offset = reader.ReadUInt32();
            uint length = reader.ReadUInt32();

            tableLocationLookup.Add(tag, offset);
        }

        reader.GoTo(tableLocationLookup["glyf"]);
        FontReader.GlyphData glyph = FontReader.ReadSimpleGlyph(reader);
        Debug.Log("Glyph: " + glyph);
    }
}
