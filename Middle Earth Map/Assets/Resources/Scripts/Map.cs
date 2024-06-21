using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private class Region
    {
        public string Name;
        public Color32 color;
        public List<Area> AreaList = new List<Area>();

        public static List<Region> RegionList = new List<Region>();

        public class Area
        {
            public string Name;
            public Color32 color;
            public int IndexInArray;
            public List<Province> ProvinceList = new List<Province>();

            public class Province
            {
                public Color32 color;
                public int Selected = 0;
                public Nation Owner;

                public static void PopulateProvinceLists()
                {
                    Area area = Area.Find("Gap of Rohan");

                    area.ProvinceList.Add(new Province(new Color32(255, 0, 0, 255), Nation.Find("Isengard")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 0, 255), Nation.Find("Isengard")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 255, 255), Nation.Find("Isengard")));

                    area.ProvinceList.Add(new Province(new Color32(255, 255, 0, 255), Nation.Find("Isengard")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 255, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 255, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 102, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(102, 102, 0, 255), Nation.Find("Isengard")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 102, 255), Nation.Find("Isengard")));

                    area = Area.Find("Fangorn");

                    area.ProvinceList.Add(new Province(new Color32(255, 0, 0, 255), Nation.Find("Fangorn")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 0, 255), Nation.Find("Fangorn")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 255, 255), Nation.Find("Fangorn")));

                    area.ProvinceList.Add(new Province(new Color32(255, 255, 0, 255), Nation.Find("Fangorn")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 255, 255), Nation.Find("Fangorn")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 255, 255), Nation.Find("Fangorn")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 0, 255), Nation.Find("Fangorn")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 0, 255), Nation.Find("Fangorn")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 102, 255), Nation.Find("Fangorn")));

                    area.ProvinceList.Add(new Province(new Color32(102, 102, 0, 255), Nation.Find("Fangorn")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 102, 255), Nation.Find("Fangorn")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 102, 255), Nation.Find("Fangorn")));

                    area.ProvinceList.Add(new Province(new Color32(255, 102, 0, 255), Nation.Find("Fangorn")));
                    area.ProvinceList.Add(new Province(new Color32(102, 255, 0, 255), Nation.Find("Fangorn")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 255, 255), Nation.Find("Fangorn")));

                    area = Area.Find("Westemnet");

                    area.ProvinceList.Add(new Province(new Color32(255, 0, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 255, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(255, 255, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 255, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 255, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 102, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(255, 102, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 255, 255), Nation.Find("Rohan")));

                    area = Area.Find("Eastemnet");

                    area.ProvinceList.Add(new Province(new Color32(255, 0, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 255, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(255, 255, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 255, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 255, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 102, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(102, 102, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 102, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 102, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(255, 102, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 102, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 255, 255), Nation.Find("Rohan")));

                    area = Area.Find("Eastfold");

                    area.ProvinceList.Add(new Province(new Color32(255, 0, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 255, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(255, 255, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 255, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 255, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 102, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(102, 102, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 102, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 102, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(255, 102, 0, 255), Nation.Find("Rohan")));

                    area = Area.Find("The Wold");

                    area.ProvinceList.Add(new Province(new Color32(255, 0, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 255, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(255, 255, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 255, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 255, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 102, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(102, 102, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 102, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 102, 255), Nation.Find("Rohan")));

                    area.ProvinceList.Add(new Province(new Color32(255, 102, 0, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 102, 255), Nation.Find("Rohan")));
                    area.ProvinceList.Add(new Province(new Color32(102, 255, 0, 255), Nation.Find("Rohan")));

                    //Gondor
                    area = Area.Find("Lossarnach");

                    area.ProvinceList.Add(new Province(new Color32(255, 0, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 255, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(255, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 255, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 255, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 102, 255), Nation.Find("Gondor")));

                    area = Area.Find("Anorien");

                    area.ProvinceList.Add(new Province(new Color32(255, 0, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 255, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(255, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 255, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 255, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 102, 255), Nation.Find("Gondor")));

                    area = Area.Find("Lebennin");

                    area.ProvinceList.Add(new Province(new Color32(255, 0, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 255, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(255, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 255, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 255, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 102, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(255, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 255, 255), Nation.Find("Gondor")));

                    area = Area.Find("Lamgedon");

                    area.ProvinceList.Add(new Province(new Color32(255, 0, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 255, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(255, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 255, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 255, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 102, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(255, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 255, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 255, 255), Nation.Find("Gondor")));

                    area = Area.Find("Belfalas");

                    area.ProvinceList.Add(new Province(new Color32(255, 0, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 255, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(255, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 255, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 255, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 0, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(102, 0, 102, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(255, 102, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(255, 0, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 255, 0, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 255, 102, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(102, 0, 255, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(0, 102, 255, 255), Nation.Find("Gondor")));

                    area.ProvinceList.Add(new Province(new Color32(255, 102, 102, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(102, 255, 102, 255), Nation.Find("Gondor")));
                    area.ProvinceList.Add(new Province(new Color32(102, 102, 255, 255), Nation.Find("Gondor")));
                }

                public Province(Color32 col, Nation owner) => (color, Owner) = (col, owner);
            }

            public static void PopulateAreaLists()
            {
                //Region region = Region.Find("Eriador");

                //region.AreaList.Add(new Area("Shire", new Color32(255, 0, 0, 255)));

                Region region = Region.Find("Rohan");

                region.AreaList.Add(new Area("Gap of Rohan", new Color32(255, 0, 0, 255)));
                region.AreaList.Add(new Area("Fangorn", new Color32(0, 255, 0, 255)));
                region.AreaList.Add(new Area("Westemnet", new Color32(0, 0, 255, 255)));

                region.AreaList.Add(new Area("Eastemnet", new Color32(255, 255, 0, 255)));
                region.AreaList.Add(new Area("Eastfold", new Color32(255, 0, 255, 255)));
                region.AreaList.Add(new Area("The Wold", new Color32(0, 255, 255, 255)));

                region = Region.Find("Gondor");

                region.AreaList.Add(new Area("Lossarnach", new Color32(255, 0, 0, 255)));
                region.AreaList.Add(new Area("Anorien", new Color32(0, 255, 0, 255)));
                region.AreaList.Add(new Area("Lebennin", new Color32(0, 0, 255, 255)));

                region.AreaList.Add(new Area("Lamgedon", new Color32(255, 255, 0, 255)));
                region.AreaList.Add(new Area("Belfalas", new Color32(0, 255, 255, 255)));

                Province.PopulateProvinceLists();
            }

            public static Area Find(string name)
            {
                foreach (Region region in RegionList)
                    if (region.AreaList.Where(A => A.Name == name).Count() > 0)
                        return region.AreaList.Where(A => A.Name == name).First();

                return null;
            }

            public Area(string name, Color32 col) => (Name, color) = (name, col);
        }

        public static void PopulateRegionList()
        {
            //RegionList.Add(new Region("Eriador", new Color32(255, 0, 0, 255)));
            RegionList.Add(new Region("Rohan", new Color32(255, 0, 0, 255)));
            RegionList.Add(new Region("Gondor", new Color32(0, 255, 0, 255)));

            Area.PopulateAreaLists();
        }

        public static Region Find(string name)
        {
            return RegionList.Where(R => R.Name == name).First();
        }

        public Region(string name, Color32 col) => (Name, color) = (name, col);
    }

    private class Nation
    {
        public string Name;
        public Color32 color;

        public static List<Nation> NationList = new List<Nation>();

        public static void PopulateNationList()
        {
            NationList.Add(new Nation("Isengard", new Color32(255, 255, 255, 255)));
            NationList.Add(new Nation("Gondor", new Color32(153, 153, 153, 255)));
            NationList.Add(new Nation("Fangorn", new Color32(0, 151, 0, 255)));
            NationList.Add(new Nation("Rohan", new Color32(153, 102, 0, 255)));
        }

        public static Nation Find(string name)
        {
            return NationList.Where(N => N.Name == name).First();
        }

        public Nation(string name, Color32 col) => (Name, color) = (name, col);
    }

    struct NationShading
    {
        public int ProvinceMapIndex;
        public int Selected;
        public Vector3 ProvinceColor;
        public Vector3 NationColor;
    }

    public void Start()
    {
        Nation.PopulateNationList();
        Region.PopulateRegionList();
        ShaderSetup();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TextRendering.ParseFont(Environment.CurrentDirectory + @"\Assets\Resources\Fonts\JetBrainsMono-Bold.ttf");
    }

    private void OnDisable()
    {
        if (ShadingBuffer != null)
            ShadingBuffer.Release();
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

        AssignAreaIndices(textures);

        Texture2DArray ProvinceTextureArray = ConvertTextureArray(textures);

        textures = Resources.LoadAll<Texture2D>("Textures/Area Maps/");

        Texture2DArray AreaTextureArray = ConvertTextureArray(textures);

        SetInitialShaderProperties(Target, ProvinceTextureArray, AreaTextureArray);

        Texture2D[] textures2 = RetrieveOutputTextures();

        AssignOutputTextures(textures2);
    }

    private Texture2DArray ConvertTextureArray(Texture2D[] textures)
    {
        Texture2DArray TextureArray = new Texture2DArray(width, height, textures.Length, TextureFormat.RGBA32, false);

        for (int i = 0; i < textures.Length; i++)
            Graphics.CopyTexture(textures[i], 0, 0, TextureArray, i, 0);

        return TextureArray;
    }

    private void AssignAreaIndices(Texture2D[] textures)
    {
        for (int x = 0; x < textures.Length; x++)
        {
            foreach (Region region in Region.RegionList)
            {
                List<Region.Area> areas = region.AreaList.Where(A => textures[x].name.Replace("_", " ") == "Province Map " + A.Name).ToList();

                if (areas.Count > 0)
                    areas[0].IndexInArray = x;
            }
        }
    }

    private void SetInitialShaderProperties(RenderTexture Target, Texture2DArray ProvinceTexs, Texture2DArray AreaTexs)
    {
        int Count = CreateNationMap();

        CompShader.SetTexture(0, "Result", Target);

        CompShader.SetTexture(0, "_ProvinceMaps", ProvinceTexs);
        CompShader.SetInt("_NumberOfProvinceMaps", ProvinceTexs.depth);

        CompShader.SetTexture(0, "_AreaMaps", AreaTexs);
        CompShader.SetInt("_NumberOfAreaMaps", AreaTexs.depth);

        CompShader.SetBuffer(0, "_ShadingData", ShadingBuffer);
        CompShader.SetInt("_ShadingDataCount", Count);

        CompShader.SetTexture(0, "_TerrainMap", Terrain);
        CompShader.SetTexture(0, "_OutlineMap", Outline);
    }

    private int CreateNationMap()
    {
        List<NationShading> NationList = new List<NationShading>();

        foreach (Region region in Region.RegionList)
            foreach (Region.Area area in region.AreaList)
                foreach (Region.Area.Province province in area.ProvinceList)
                    NationList.Add(new NationShading()
                    {
                        ProvinceMapIndex = area.IndexInArray,
                        Selected = province.Selected,
                        NationColor = new Vector3(province.Owner.color.r / 255f, province.Owner.color.g / 255f, province.Owner.color.b / 255f),
                        ProvinceColor = new Vector3(province.color.r / 255f, province.color.g / 255f, province.color.b / 255f)
                    });

        ShadingBuffer = new ComputeBuffer(NationList.Count, 32);
        ShadingBuffer.SetData(NationList);

        return NationList.Count;
    }

    private Texture2D[] RetrieveOutputTextures()
    {
        List<Texture2D> TextureList = new List<Texture2D>();

        for (int x = 0; x < 4; x++)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);

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
        MapMaterial.SetTexture("_Terrain", textures[3]);
    }
}
