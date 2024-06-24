using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextRenderingMono : MonoBehaviour
{
    TextRendering.FontReader.GlyphData glyph = null;

    private ComputeBuffer PointBuffer;
    private ComputeBuffer ContourEndIndicesBuffer;

    private void Start()
    {
        if (glyph == null)
            glyph = TextRendering.ParseFont(Environment.CurrentDirectory + @"\Assets\Resources\Fonts\JetBrainsMono-Bold.ttf");
    }

    private void OnDrawGizmos()
    {
        if (glyph != null)
            TextRendering.FontReader.GlyphData.GlyphDrawTest(glyph);
    }

    private void PopulateGlyphBuffers()
    {
        PointBuffer = new ComputeBuffer(glyph.Points.Count(), 12);
        PointBuffer.SetData(glyph.Points);

        ContourEndIndicesBuffer = new ComputeBuffer(glyph.ContourEndIndices.Count(), 4);
        ContourEndIndicesBuffer.SetData(glyph.ContourEndIndices);
    }
}
