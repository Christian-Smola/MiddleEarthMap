using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore;

public class TextRendering
{
    public static List<GlyphData> Glyphs = new List<GlyphData>();

    public class FontReader : IDisposable
    {
        private readonly Stream stream;
        private readonly BinaryReader reader;

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

        public double ReadFixedPoint2Dot14() => UInt16ToFixedPoint2Dot14(ReadUInt16());

        public static double UInt16ToFixedPoint2Dot14(UInt16 raw)
        {
            return (Int16)(raw) / (double)(1 << 14);
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

        public static void ReadCoordinates(in GlyphData.Point[] points, FontReader reader, byte[] allFlags, bool readingX)
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
                points[i].OnCurve = FlagBitIsSet(currFlag, 0) ? 1 : 0;

                min = Math.Min(min, coordVal);
                max = Math.Max(max, coordVal);
            }
        }

        public void Dispose() { }

        public byte ReadByte() => reader.ReadByte();

        public sbyte ReadSByte() => reader.ReadSByte();

        public Int32 ReadInt16() => (Int16)ReadUInt16();

        public void Skip16BitEntries(int num) => SkipBytes(num * 2);

        public void SkipBytes(int num) => reader.BaseStream.Seek(num, SeekOrigin.Current);

        public void GoTo(uint OffsetFromOrigin) => reader.BaseStream.Seek(OffsetFromOrigin, SeekOrigin.Begin);

        public void GoTo(long OffserFromOrigin) => reader.BaseStream.Seek(OffserFromOrigin, SeekOrigin.Begin);

        public uint GetLocation() => (uint)reader.BaseStream.Position;

        public static bool FlagBitIsSet(byte flag, int index) => ((flag >> index) & 1) == 1;

        public static bool FlagBitIsSet(uint flag, int index) => ((flag >> index) & 1) == 1;
    }

    public class GlyphData
    {
        public Point[] Points;
        public int[] ContourEndIndices;

        public uint UnicodeValue;
        public uint GlyphIndex;

        public int MinX;
        public int MaxX;
        public int MinY;
        public int MaxY;

        public struct Point
        {
            public float X;
            public float Y;
            public int OnCurve;

            public Vector2 ToVec2() => new Vector2(X, Y);

            public Vector3 ToVec3() => new Vector3(X, Y, 10);

            public Point(int x, int y) : this() => (X, Y) = (x, y);

            public Point(int x, int y, int onCurve) => (X, Y, OnCurve) = (x, y, onCurve);
        }

        private static GlyphData ReadGlyph(FontReader reader, uint[] glyphLocations, uint glyphIndex)
        {
            uint glyphLocation = glyphLocations[glyphIndex];

            reader.GoTo(glyphLocation);
            int contourCount = reader.ReadInt16();

            bool isSimpleGlyph = contourCount >= 0;

            if (isSimpleGlyph)
                return ReadSimpleGlyph(reader, glyphLocations, glyphIndex);
            else
                return ReadCompoundGlyph(reader, glyphLocations, glyphIndex);
        }

        public static GlyphData[] ReadAllGlyphs(FontReader reader, uint[] glyphLocations, GlyphMap[] mappings)
        {
            GlyphData[] glyphs = new GlyphData[mappings.Length];

            for (int i = 0; i < mappings.Length; i++)
            {
                GlyphMap mapping = mappings[i];

                GlyphData glyphData = ReadGlyph(reader, glyphLocations, mapping.GlyphIndex);
                glyphData.UnicodeValue = mapping.Unicode;
                glyphs[i] = glyphData;
            }

            return glyphs;
        }

        public static GlyphData ReadSimpleGlyph(FontReader reader, uint[] glyphLocations, uint glyphIndex)
        {
            const int OnCurve = 0;
            const int IsSingleByteX = 1;
            const int IsSingleByteY = 2;
            const int Repeat = 3;
            const int InstructionX = 4;
            const int InstructionY = 5;

            reader.GoTo(glyphLocations[glyphIndex]);

            GlyphData glyphData = new();
            glyphData.GlyphIndex = glyphIndex;

            int contourCount = reader.ReadInt16();
            if (contourCount < 0) throw new Exception("Expected simple glyph, but found compound glyph instead");

            glyphData.MinX = reader.ReadInt16();
            glyphData.MinY = reader.ReadInt16();
            glyphData.MaxX = reader.ReadInt16();
            glyphData.MaxY = reader.ReadInt16();

            int numPoints = 0;
            int[] contourEndIndices = new int[contourCount];

            for (int i = 0; i < contourCount; i++)
            {
                int contourEndIndex = reader.ReadUInt16();
                numPoints = Math.Max(numPoints, contourEndIndex + 1);
                contourEndIndices[i] = contourEndIndex;
            }

            int instructionsLength = reader.ReadInt16();
            reader.SkipBytes(instructionsLength);

            byte[] allFlags = new byte[numPoints];
            Point[] points = new Point[numPoints];


            for (int i = 0; i < numPoints; i++)
            {
                byte flag = reader.ReadByte();
                allFlags[i] = flag;

                if (FontReader.FlagBitIsSet(flag, Repeat))
                {
                    int repeatCount = reader.ReadByte();

                    for (int r = 0; r < repeatCount; r++)
                    {
                        i++;
                        allFlags[i] = flag;
                    }
                }
            }

            FontReader.ReadCoordinates(points, reader, allFlags, true);
            FontReader.ReadCoordinates(points, reader, allFlags, false);

            glyphData.Points = points;
            glyphData.ContourEndIndices = contourEndIndices;

            return glyphData;

            //int[] contourEndIndices = new int[reader.ReadUInt16()];
            //reader.SkipBytes(8);

            //for (int i = 0; i < contourEndIndices.Length; i++)
            //    contourEndIndices[i] = reader.ReadUInt16();

            //int numPoints = contourEndIndices[^1] + 1;
            //byte[] allFlags = new byte[numPoints];
            //reader.SkipBytes(reader.ReadUInt16());

            //for (int i = 0; i < numPoints; i++)
            //{
            //    byte flag = reader.ReadByte();
            //    allFlags[i] = flag;

            //    if (FontReader.FlagBitIsSet(flag, 3))
            //    {
            //        for (int r = 0; r < reader.ReadByte(); r++)
            //        {
            //            i++;
            //            allFlags[i] = flag;
            //        }
            //    }
            //}

            //Point[] points = new Point[numPoints];

            //FontReader.ReadCoordinates(points, reader, allFlags, true);
            //FontReader.ReadCoordinates(points, reader, allFlags, false);

            //return new GlyphData(points, contourEndIndices);
        }

        public static GlyphData ReadCompoundGlyph(FontReader reader, uint[] glyphLocations, uint glyphIndex)
        {
            GlyphData compoundGlyph = new();
            compoundGlyph.GlyphIndex = glyphIndex;

            uint glyphLocation = glyphLocations[glyphIndex];
            reader.GoTo(glyphLocation);
            reader.SkipBytes(2);

            compoundGlyph.MinX = reader.ReadInt16();
            compoundGlyph.MinY = reader.ReadInt16();
            compoundGlyph.MaxX = reader.ReadInt16();
            compoundGlyph.MaxY = reader.ReadInt16();

            List<Point> allPoints = new List<Point>();
            List<int> allcontourEndIndices = new List<int>();

            while (true)
            {
                (GlyphData componentGlyph, bool hasMoreGlyphs) = ReadNextComponentGlyph(reader, glyphLocations, glyphLocation);

                foreach (int endIndex in componentGlyph.ContourEndIndices)
                    allcontourEndIndices.Add(endIndex + allPoints.Count);

                allPoints.AddRange(componentGlyph.Points);

                if (!hasMoreGlyphs)
                    break;
            }

            compoundGlyph.Points = allPoints.ToArray();
            compoundGlyph.ContourEndIndices = allcontourEndIndices.ToArray();
            return compoundGlyph;
        }

        private static (GlyphData glyph, bool hasMoreGlyphs) ReadNextComponentGlyph(FontReader reader, uint[] glyphLocations, uint glyphLocation)
        {
            uint flag = reader.ReadUInt16();
            uint glyphIndex = reader.ReadUInt16();

            uint componentGlyphLocation = glyphLocations[glyphIndex];

            if (componentGlyphLocation == glyphLocation)
                return (new GlyphData(Array.Empty<Point>(), Array.Empty<int>()), false);

            bool argsAre2Bytes = FontReader.FlagBitIsSet(flag, 0);
            bool argsAreXYValues = FontReader.FlagBitIsSet(flag, 1);
            bool roundXYToGrid = FontReader.FlagBitIsSet(flag, 2);
            bool isSingleScaleValue = FontReader.FlagBitIsSet(flag, 3);
            bool isMoreComponentsAfterThis = FontReader.FlagBitIsSet(flag, 5);
            bool isXAndYScale = FontReader.FlagBitIsSet(flag, 6);
            bool is2x2Matrix = FontReader.FlagBitIsSet(flag, 7);
            bool hasInstructions = FontReader.FlagBitIsSet(flag, 8);
            bool useThisComponentMetrics = FontReader.FlagBitIsSet(flag, 9);
            bool componentsOverlap = FontReader.FlagBitIsSet(flag, 10);

            int arg1 = argsAre2Bytes ? reader.ReadInt16() : reader.ReadSByte();
            int arg2 = argsAre2Bytes ? reader.ReadInt16() : reader.ReadSByte();

            if (!argsAreXYValues) throw new Exception("TODO: Args1&2 are point indices to be matched, rather than offsets");

            double offsetX = arg1;
            double offsetY = arg2;

            double iHat_x = 1;
            double iHat_y = 0;
            double jHat_x = 0;
            double jHat_y = 1;

            if (isSingleScaleValue)
            {
                iHat_x = reader.ReadFixedPoint2Dot14();
                jHat_y = iHat_x;
            }
            else if (isXAndYScale)
            {
                iHat_x = reader.ReadFixedPoint2Dot14();
                jHat_y = reader.ReadFixedPoint2Dot14();
            }
            // Todo: incomplete implemntation
            else if (is2x2Matrix)
            {
                iHat_x = reader.ReadFixedPoint2Dot14();
                iHat_y = reader.ReadFixedPoint2Dot14();
                jHat_x = reader.ReadFixedPoint2Dot14();
                jHat_y = reader.ReadFixedPoint2Dot14();
            }

            uint currentCompoundGlyphReadLocation = reader.GetLocation();
            GlyphData simpleGlyph = ReadGlyph(reader, glyphLocations, glyphIndex);
            reader.GoTo(currentCompoundGlyphReadLocation);

            for (int i = 0; i < simpleGlyph.Points.Length; i++)
            {
                (double xPrime, double yPrime) = TransformPoint(simpleGlyph.Points[i].X, simpleGlyph.Points[i].Y);
                simpleGlyph.Points[i].X = (int)xPrime;
                simpleGlyph.Points[i].Y = (int)yPrime;
            }

            return (simpleGlyph, isMoreComponentsAfterThis);

            (double xPrime, double yPrime) TransformPoint(double x, double y)
            {
                double xPrime = iHat_x * x + jHat_x * y + offsetX;
                double yPrime = iHat_y * x + jHat_y * y + offsetY;
                return (xPrime, yPrime);
            }
        }

        public static uint[] GetAllGlyphLocations(FontReader reader, int numOfGlyphs, int bytesPerEntry, uint locaTableLocation, uint glyfTableLocation)
        {
            uint[] allGlyphLocations = new uint[numOfGlyphs];
            bool isTwoByteEntry = bytesPerEntry == 2;

            for (int i = 0; i < numOfGlyphs; i++)
            {
                reader.GoTo(locaTableLocation + i * bytesPerEntry);
                uint glyphDataOffset = isTwoByteEntry ? reader.ReadUInt16() * 2u : reader.ReadUInt32();
                allGlyphLocations[i] = glyfTableLocation + glyphDataOffset;
            }

            return allGlyphLocations;
        }

        public static void GlyphDrawTest(GlyphData glyph)
        {
            int contourStartIndex = 0;

            foreach (int contourEndIndex in glyph.ContourEndIndices)
            {
                int numPointsInContour = contourEndIndex - contourStartIndex + 1;

                Span<Point> points = glyph.Points.AsSpan(contourStartIndex, numPointsInContour);

                for (int i = 0; i < points.Length; i += 2)
                {
                    Point p1 = points[i];
                    Point p2 = points[(i + 1) % points.Length];
                    Point p3 = points[(i + 2) % points.Length];
                    DrawBezier(p1.ToVec2(), p2.ToVec2(), p3.ToVec2(), 30);
                }
                contourStartIndex = contourEndIndex + 1;
            }

            //for (int i = 0; i < glyph.Points.Length; i++)
            //    Gizmos.DrawPoint(glyph.Points[i]);
        }

        public static void DrawBezier(Vector2 p1, Vector2 p2, Vector2 p3, int resolution)
        {
            Vector2 previousPointOnCurve = p1;

            for (int i = 0; i < resolution; i++)
            {
                float t = (i + 1f) / resolution;
                Vector2 nextPointOnCurve = BezierInterpolation(p1, p2, p3, t);
                Gizmos.DrawLine(previousPointOnCurve, nextPointOnCurve);
                previousPointOnCurve = nextPointOnCurve;
            }
        }

        public GlyphData()
        {

        }

        public GlyphData(Point[] points, int[] EndPoints) => (Points, ContourEndIndices) = (points, EndPoints);
    }

    public class FontData
    {
        public GlyphData[] Glyphs { get; private set; }
        public GlyphData MissingGlyph;
        public int UnitsPerEm;

        Dictionary<uint, GlyphData> glyphLookup;

        public static FontData Parse(string path)
        {
            using FontReader reader = new FontReader(path);

            Dictionary<string, uint> tableLocationLookup = ReadTableLocations(reader);

            uint glyphTableLocation = tableLocationLookup["glyf"];
            uint locaTableLocation = tableLocationLookup["loca"];
            uint cmapLocation = tableLocationLookup["cmap"];

            reader.GoTo(tableLocationLookup["head"]);
            reader.SkipBytes(18);

            int unitsPerEm = reader.ReadInt16();
            reader.SkipBytes(30);

            int numBytesPerLocationLookup = (reader.ReadInt16() == 0 ? 2 : 4);

            reader.GoTo(tableLocationLookup["maxp"]);
            reader.SkipBytes(4);

            int numGlyphs = reader.ReadUInt16();
            uint[] glyphLocations = GlyphData.GetAllGlyphLocations(reader, numGlyphs, numBytesPerLocationLookup, locaTableLocation, glyphTableLocation);

            GlyphMap[] mappings = GetUnicodeToGlyphIndexMappings(reader, cmapLocation);
            GlyphData[] glyphs = GlyphData.ReadAllGlyphs(reader, glyphLocations, mappings);

            //ApplyLayoutInfo();

            FontData fontData = new FontData(glyphs, unitsPerEm);
            return fontData;
        }

        private static Dictionary<string, uint> ReadTableLocations(FontReader reader)
        {
            Dictionary<string, uint> tableLocations = new Dictionary<string, uint>();

            reader.SkipBytes(4);
            int numTables = reader.ReadUInt16();
            reader.SkipBytes(6);

            for (int i = 0; i < numTables; i++)
            {
                string tag = reader.ReadTag();
                uint checksum = reader.ReadUInt32();
                uint offset = reader.ReadUInt32();
                uint length = reader.ReadUInt32();

                tableLocations.Add(tag, offset);
            }

            return tableLocations;
        }

        private static GlyphMap[] GetUnicodeToGlyphIndexMappings(FontReader reader, uint cmapOffset)
        {
            List<GlyphMap> glyphPairs = new List<GlyphMap>();
            reader.GoTo(cmapOffset);

            uint version = reader.ReadUInt16();
            uint numSubtables = reader.ReadUInt16();

            uint cmapSubtableOffset = 0;
            int selectedUnicodeVersionID = -1;

            for (int i = 0; i < numSubtables; i++)
            {
                int platformID = reader.ReadInt16();
                int platformSpecificID = reader.ReadInt16();
                uint offset = reader.ReadUInt32();

                if (platformID == 0)
                {
                    //Unicode encoding
                    if (platformSpecificID is 0 or 1 or 3 or 4 && platformSpecificID > selectedUnicodeVersionID)
                    {
                        cmapSubtableOffset = offset;
                        selectedUnicodeVersionID = platformSpecificID;
                    }
                }
                else if (platformID == 3 && selectedUnicodeVersionID == -1)
                    if (platformSpecificID is 1 or 10)
                        cmapSubtableOffset = offset;
            }

            if (cmapSubtableOffset == 0)
                throw new Exception("Font does not contain supported character map type");

            reader.GoTo(cmapOffset + cmapSubtableOffset);
            int format = reader.ReadInt16();
            bool hasReadMissingCharGlyph = false;

            if (format != 12 && format != 4)
                throw new Exception("Font cmap format not supported: " + format);

            if (format == 4)
            {
                int length = reader.ReadUInt16();
                int languageCode = reader.ReadUInt16();

                int segCount2X = reader.ReadUInt16();
                int segCount = segCount2X / 2;

                reader.SkipBytes(6);

                int[] endCodes = new int[segCount];

                for (int i = 0; i < segCount; i++)
                    endCodes[i] = reader.ReadInt16();

                reader.Skip16BitEntries(1);

                int[] startCodes = new int[segCount];

                for (int i = 0; i < segCount; i++)
                    startCodes[i] = reader.ReadInt16();

                int[] idDeltas = new int[segCount];

                for (int i = 0; i < segCount; i++)
                    idDeltas[i] = reader.ReadInt16();

                (int offset, int readLoc)[] idRangeOffsets = new (int, int)[segCount];
                for (int i = 0; i < segCount; i++)
                {
                    int readLoc = (int)reader.GetLocation();
                    int offset = reader.ReadInt16();
                    idRangeOffsets[i] = (offset, readLoc);
                }

                for (int i = 0; i < startCodes.Length; i++)
                {
                    int endCode = endCodes[i];
                    int currCode = startCodes[i];

                    if (currCode == 65535) break;

                    while (currCode <= endCode)
                    {
                        int glyphIndex;

                        if (idRangeOffsets[i].offset == 0)
                            glyphIndex = (currCode + idDeltas[i]) % 65536;
                        else
                        {
                            uint readerLocationOld = reader.GetLocation();
                            int rangeoffsetLocation = idRangeOffsets[i].readLoc + idRangeOffsets[i].offset;
                            int glyphIndexArrayLocation = 2 * (currCode - startCodes[i]) + rangeoffsetLocation;

                            reader.GoTo(glyphIndexArrayLocation);
                            glyphIndex = reader.ReadInt16();

                            if (glyphIndex != 0)
                                glyphIndex = (glyphIndex + idDeltas[i]) % 65536;

                            reader.GoTo(readerLocationOld);
                        }

                        glyphPairs.Add(new GlyphMap((uint)glyphIndex, (uint)currCode));
                        hasReadMissingCharGlyph |= glyphIndex == 0;
                        currCode++;
                    }
                }
            }
            else if (format == 12)
            {
                reader.SkipBytes(10);
                uint numGroups = reader.ReadUInt32();

                for (int i = 0; i < numGroups; i++)
                {
                    uint startCharCode = reader.ReadUInt32();
                    uint endCharCode = reader.ReadUInt32();
                    uint startGlyphIndex = reader.ReadUInt32();

                    uint numChars = endCharCode - startCharCode + 1;
                    for (int charCodeOffset = 0; charCodeOffset < numChars; charCodeOffset++)
                    {
                        uint charCode = (uint)(startCharCode + charCodeOffset);
                        uint glyphIndex = (uint)(startGlyphIndex + charCodeOffset);

                        glyphPairs.Add(new GlyphMap(glyphIndex, charCode));
                        hasReadMissingCharGlyph |= glyphIndex == 0;
                    }
                }
            }

            if (!hasReadMissingCharGlyph)
                glyphPairs.Add(new GlyphMap(0, 65535));

            return glyphPairs.ToArray();
        }

        public FontData(GlyphData[] glyphs, int unitsPerEm)
        {
            Glyphs = glyphs;
            UnitsPerEm = unitsPerEm;
            glyphLookup = new();

            foreach (GlyphData data in glyphs)
            {
                if (data == null)
                    continue;

                glyphLookup.Add(data.UnicodeValue, data);

                if (data.GlyphIndex == 0)
                    MissingGlyph = data;
            }
        }
    }

    public readonly struct GlyphMap
    {
        public readonly uint GlyphIndex;
        public readonly uint Unicode;

        public GlyphMap(uint index, uint unicode) => (GlyphIndex, Unicode) = (index, unicode);
    }

    public static Vector2 BezierInterpolation(Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        Vector2 intermediateA = LinearInterpolation(p1, p2, t);
        Vector2 intermediateB = LinearInterpolation(p2, p3, t);

        return LinearInterpolation(intermediateA, intermediateB, t);
    }

    public static Vector2 LinearInterpolation(Vector2 start, Vector2 end, float t) => start + (end - start) * t;
}
