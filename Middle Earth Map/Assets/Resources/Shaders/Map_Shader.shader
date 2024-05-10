Shader "Custom/Map_Shader"
{
    Properties
    {
        _Terrain ("Terrain Map", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}

        _OceanMap("Ocean Map", 2D) = "white" {}
        _OceanCol("Ocean Color", 2D) = "white" {}

        _ProvinceMap ("Province Map", 2D) = "white" {}
        _AreaMap ("Area Map", 2D) = "white" {}

        _ProvinceOutlines ("Province Outlines", 2D) = "white" {}

        _NationMap ("Nation Map", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "TerrainCompatible" = "True"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct Wave
            {
                float2 dir;
                float steepness;
                float length;
            };

            Wave ConstructWave(float2 d, float s, float l)
            {
                Wave w;

                w.dir = d;
                w.steepness = s;
                w.length = l;

                return w;
            }

            sampler2D _Terrain;
            float4 _Terrain_ST;

            sampler2D _NormalMap;
            float4 _NormalMap_ST;

            sampler2D _OceanMap;
            float4 _OceanMap_ST;

            sampler2D _OceanCol;
            float4 _OceanCol_ST;

            //based on the following article by Catlike Coding
            //https://catlikecoding.com/unity/tutorials/flow/waves/
            float3 GerstnerWaves(Wave wave, float3 p, inout float3 tangent, inout float3 binormal)
            {
                float k = 2 * UNITY_PI / wave.length;
                float c = sqrt(9.8 / k);
                float2 d = normalize(wave.dir);
                float f = k * (dot(d, p.xz) - c * _Time.y);
                float a = wave.steepness / k;

                tangent += float3(
                    -d.x * d.x * (wave.steepness * sin(f)),
                    d.x * (wave.steepness * cos(f)),
                    -d.x * d.y * (wave.steepness * sin(f))
                    );

                binormal += float3(
                    -d.x * d.y * (wave.steepness * sin(f)),
                    d.y * (wave.steepness * cos(f)),
                    -d.y * d.y * (wave.steepness * sin(f))
                    );

                return float3(d.x * (a * cos(f)), a * sin(f), d.y * (a * cos(f)));
            }

            float4 GenerateOcean(float3 v)
            {
                Wave waveA = ConstructWave(float2(1, -1), 20, 0.6f);
                //Wave waveB = ConstructWave(float2(1, 0.4f), 2.0f, 4);

                float3 gridPoint = v;
                float3 tangent = float3(1, 0, 0);
                float3 binormal = float3(0, 0, 1);
                float3 p = gridPoint;

                p += GerstnerWaves(waveA, gridPoint, tangent, binormal);
                //p += GerstnerWaves(waveB, gridPoint, tangent, binormal);

                float3 normal = normalize(cross(binormal, tangent));

                return float4(p, 1);
            }

            v2f vert(appdata v)
            {
                v2f o;

                //float4 Ocean = tex2Dlod(_OceanMap, float4(v.uv, 0, 0));

                //v.vertex.xyz = lerp(v.vertex.xyz, GenerateOcean(v.vertex.xyz), Ocean.a > 0.1f);

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _Terrain);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 Terrain = tex2D(_Terrain, i.uv);
                float4 Normal = tex2D(_NormalMap, i.uv);

                float4 Ocean = tex2D(_OceanMap, i.uv);
                float4 OceanCol = tex2D(_OceanCol, i.uv);

                float3 SunDir = _WorldSpaceLightPos0.xyz;
                float3 shading = saturate(saturate(dot(Normal, SunDir) + 0.05)) * 5;

                Terrain = lerp(Terrain, float4(0, 0, 0, 0), Ocean.a > 0.1);

                clip(Terrain.a - 0.5f);

                return Terrain;
            }

            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _NationMap;
            float4 _NationMap_ST;

            sampler2D _ProvinceMap;
            float4 _ProvinceMap_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 Provinces = tex2D(_ProvinceMap, i.uv);
                float4 Nations = tex2D(_NationMap, i.uv);

                float4 Output = lerp(Provinces, Nations, Nations.a > 0.5f);

                clip(Output.a - 0.5f);

                return float4(Output.rgb, 0.5f);
            }
            ENDCG
        }
    }
}
