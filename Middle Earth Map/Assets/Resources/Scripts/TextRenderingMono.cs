using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore;

public class TextRenderingMono : MonoBehaviour
{
    private int width = 500, height = 500;

    private RenderTexture Target;
    private RenderTexture Buffer;

    public ComputeShader CompShader;
    public Material TextRenderingMat;

    private ComputeBuffer PointBuffer;
    private ComputeBuffer ContourEndIndicesBuffer;

    private TextRendering.FontData fontData;

    private void Start()
    {
        //fontData = TextRendering.FontData.Parse(Environment.CurrentDirectory + @"\Assets\Resources\Fonts\JetBrainsMono-Bold.ttf");

        //glyph = TextRendering.Glyphs[1];

        //ShaderSetup();

        TextRendering.DickButts();
    }

    private void OnDrawGizmos()
    {
        if (TextRendering.Glyphs.Count > 0)
        {
            string text = "Hello World";
            Vector2 offset = Vector2.zero;

            foreach (char character in text)
            {
                if (character == ' ')
                {
                    offset.x += 2;
                    continue;
                }

                //TextRendering.GlyphData glyphData = fontData.AllGlyphs[character];

                //TextRendering.GlyphData.GlyphDrawTest(glyph, offset);

                offset.x = 1;
            }
        }
    }

    //private void PopulateGlyphBuffers()
    //{
    //    PointBuffer = new ComputeBuffer(glyph.Points.Count(), 12);
    //    PointBuffer.SetData(glyph.Points);

    //    ContourEndIndicesBuffer = new ComputeBuffer(glyph.ContourEndIndices.Count(), 4);
    //    ContourEndIndicesBuffer.SetData(glyph.ContourEndIndices);
    //}

    //private void ShaderSetup()
    //{
    //    if (Buffer is null)
    //        Buffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

    //    if (Target is null)
    //    {
    //        Target = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
    //        Target.enableRandomWrite = true;
    //        Target.Create();
    //    }

    //    PopulateGlyphBuffers();

    //    SetInitialShaderProperties(Target);

    //    Texture2D[] textures = RetrieveOutputTextures();

    //    AssignOutputTextures(textures);
    //}

    //private void SetInitialShaderProperties(RenderTexture Target)
    //{
    //    CompShader.SetTexture(0, "Result", Target);

    //    CompShader.SetBuffer(0, "_Points", PointBuffer);
    //    CompShader.SetInt("_PointsCount", glyph.Points.Count());

    //    CompShader.SetBuffer(0, "ContourEndIndices", ContourEndIndicesBuffer);
    //    CompShader.SetInt("_IndicesCount", glyph.ContourEndIndices.Count());
    //}

    //private Texture2D[] RetrieveOutputTextures()
    //{
    //    List<Texture2D> TextureList = new List<Texture2D>();

    //    for (int x = 0; x < 1; x++)
    //    {
    //        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);

    //        CompShader.Dispatch(0, width, height, 1);

    //        Graphics.Blit(Target, Buffer);
    //        RenderTexture.active = Buffer;

    //        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
    //        texture.Apply();

    //        TextureList.Add(texture);
    //    }

    //    return TextureList.ToArray();
    //}

    //private void AssignOutputTextures(Texture2D[] textures)
    //{
    //    TextRenderingMat.SetTexture("_MainTex", textures[0]);
    //}
}
