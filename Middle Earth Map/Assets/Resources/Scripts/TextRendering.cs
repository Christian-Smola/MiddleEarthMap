using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore;

public class TextRendering
{
    public class FontReader : IDisposable
    {
        private readonly Stream stream;
        private readonly BinaryReader reader;

        public class GlyphData
        {
            public Point[] Points;
            public int[] ContourEndIndices;

            public struct Point
            {
                public int X;
                public int Y;
                public bool OnCurve;

                public Point(int x, int y) : this() => (X, Y) = (x, y);

                public Point(int x, int y, bool onCurve) => (X, Y, OnCurve) = (x, y, onCurve);
            }

            public GlyphData(Point[] points, int[] EndPoints) => (Points, ContourEndIndices) = (points, EndPoints);
        }

        public FontReader(string Path)
        {
            stream = File.Open(Path, FileMode.Open);
            reader = new BinaryReader(stream);
        }

        public string ReadTag()
        {
            Span<char> tag = stackalloc char[4];

            for (int i = 0; i < tag.Length; i++)
                tag[i] = (char)reader.ReadByte();

            return tag.ToString();
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
                const byte mask = 0b11111111;

                UInt32 a = (value >> 24) & mask;
                UInt32 b = (value >> 16) & mask;
                UInt32 c = (value >> 8) & mask;
                UInt32 d = (value >> 0) & mask;

                value = a << 0 | b << 8 | c << 16 | d << 24;
            }

            return value;
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
                {
                    for (int r = 0; r < reader.ReadByte(); r++)
                    {
                        i++;
                        allFlags[i] = flag;
                    }
                }
            }

            GlyphData.Point[] points = new GlyphData.Point[numPoints];

            ReadCoordinates(points, reader, allFlags, true);
            ReadCoordinates(points, reader, allFlags, false);

            return new GlyphData(points, contourEndIndices);
        }

        static void ReadCoordinates(in GlyphData.Point[] points, FontReader reader, byte[] allFlags, bool readingX)
        {
            int min = int.MaxValue;
            int max = int.MinValue;

            int offsetSizeFlagBit = readingX ? 1 : 2;
            int offsetSignOrSkipBit = readingX ? 4 : 5;

            int coordVal = 0;

            for (int i = 0; i < points.Count(); i++)
            {
                byte currFlag = allFlags[i];

                if (FlagBitIsSet(currFlag, offsetSizeFlagBit))
                {
                    int coordOffset = reader.ReadByte();
                    bool positiveOffset = FlagBitIsSet(currFlag, offsetSignOrSkipBit);
                    coordVal += positiveOffset ? coordOffset : -coordOffset;
                }
                else if (!FlagBitIsSet(currFlag, offsetSignOrSkipBit))
                {
                    coordVal += reader.ReadInt16();
                }

                if (readingX) points[i].X = coordVal;
                else points[i].Y = coordVal;
                points[i].OnCurve = FlagBitIsSet(currFlag, 0);

                min = Math.Min(min, coordVal);
                max = Math.Max(max, coordVal);
            }
        }

        public void Dispose() { }

        public byte ReadByte() => reader.ReadByte();

        public Int32 ReadInt16() => (Int16)ReadUInt16();

        public void SkipBytes(int num) => reader.BaseStream.Seek(num, SeekOrigin.Current);

        public void GoTo(uint OffsetFromOrigin) => reader.BaseStream.Seek(OffsetFromOrigin, SeekOrigin.Begin);

        static bool FlagBitIsSet(byte flag, int index) => ((flag >> index) & 1) == 1;
    }

    public static void ParseFont(string Path)
    {
        using FontReader reader = new FontReader(Path);

        reader.SkipBytes(4);
        UInt16 numTables = reader.ReadUInt16();
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

        Debug.Log("Glyph 0:");

        for (int i = 0; i < glyph.ContourEndIndices.Count(); i++)
            Debug.Log(" Contour End Index " + i + ": " + glyph.ContourEndIndices[i]);

        for (int i = 0; i < glyph.Points.Count(); i++)
            Debug.Log(" Point " + i + ": (" + glyph.Points[i].X + ", " + glyph.Points[i].Y + ")"); 
    }
}
