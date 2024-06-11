using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextRenderingMono : MonoBehaviour
{
    TextRendering.FontReader.GlyphData glyph = null;

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
}
