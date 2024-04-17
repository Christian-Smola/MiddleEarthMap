using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private int width = 3172, height = 2160;

    private RenderTexture Target;
    private RenderTexture Buffer;

    public Texture2D Terrain;
    public Texture2D Outline;

    public ComputeShader CompShader;
    public Material MapMaterial;

    private ComputeBuffer ShadingBuffer;

    struct NationShading
    {
        public int ProvinceMapIndex;
        public int Selected;
        public Vector3 ProvinceColor;
        public Vector3 NationColor;
    }

    public void Start()
    {
        ShaderSetup();
    }

    private void ShaderSetup()
    {
        if (Buffer is null)
            Buffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

        if (Target is null)
        {
            Target = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            Target.enableRandomWrite = true;
            Target.Create();
        }

        Texture2D[] textures = Resources.LoadAll<Texture2D>("Textures/Province Maps/");

        Texture2DArray ProvinceTextureArray = ConvertTextureArray(textures);

        //AssignAreaIndices(textures);

        textures = Resources.LoadAll<Texture2D>("Textures/Area Maps/");

        Texture2DArray AreaTextureArray = ConvertTextureArray(textures);

        SetInitialShaderProperties(ProvinceTextureArray, AreaTextureArray);

        textures = RetrieveOutputTextures();

        AssignOutputTextures(textures);
    }

    private Texture2DArray ConvertTextureArray(Texture2D[] textures)
    {
        Texture2DArray TextureArray = new Texture2DArray(width, height, textures.Length, TextureFormat.RGBA32, false);

        for (int i = 0; i < textures.Length; i++)
            Graphics.CopyTexture(textures[i], 0, 0, TextureArray, i, 0);

        return TextureArray;
    }

    //private void AssignAreaIndices(Texture2D[] textures)
    //{
    //    for (int x = 0; x < textures.Length; x++)
    //    {
    //        foreach
    //    }
    //}

    private void SetInitialShaderProperties(Texture2DArray ProvinceTexs, Texture2DArray AreaTexs)
    {
        int Count = CreateNationMap();

        CompShader.SetTexture(0, "Result", Target);

        CompShader.SetTexture(0, "_ProvinceMaps", ProvinceTexs);
        CompShader.SetInt("_NumberOfProvinceMaps", ProvinceTexs.depth);

        CompShader.SetBuffer(0, "_ShadingData", ShadingBuffer);
        CompShader.SetInt("_ShadingDataCount", Count);

        CompShader.SetTexture(0, "_TerrainMap", Terrain);
        CompShader.SetTexture(0, "_OutlineMap", Outline);
    }

    private int CreateNationMap()
    {
        return 0;
    }

    private Texture2D[] RetrieveOutputTextures()
    {
        List<Texture2D> TextureList = new List<Texture2D>();

        for (int x = 0; x < 4; x++)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            CompShader.SetInt("_OutputValue", x);

            CompShader.Dispatch(0, width, height, 1);

            Graphics.Blit(Target, Buffer);
            RenderTexture.active = Buffer;

            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            texture.Apply();

            TextureList.Add(texture);
        }

        return TextureList.ToArray();
    }

    private void AssignOutputTextures(Texture2D[] textures)
    {
        MapMaterial.SetTexture("_ProvinceMap", textures[0]);
        MapMaterial.SetTexture("_AreaMap", textures[1]);
        MapMaterial.SetTexture("_NationMap", textures[2]);
        MapMaterial.SetTexture("_TerrainMap", textures[3]);
    }
}