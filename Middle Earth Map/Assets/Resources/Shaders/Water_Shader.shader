Shader "Custom/Water_Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OceanMap ("Ocean Map", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "TerrainCompatible" = "True" }
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _OceanMap;
            float4 _OceanMap_ST;

            float4 SimpleWaves(float x, float z)
            {
                float k = 2;
                float w = 2;
                float t = _Time[1];
                float A = 50; //amplitude

                //Original Equation
                //float Equation = A * sin(kz * z + kx * x + w * t);

                float2 v = float2(x, z) * k;

                float Equation = A * sin(v.x + v.y + w * t);

                return float4(0, Equation, 0, 0);
            }

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

            v2f vert (appdata v)
            {
                v2f o;

                Wave waveA = ConstructWave(float2(1, 1),20, 0.8f);
                //Wave waveB = ConstructWave(float2(1, 0.2f), 6.0f, 8);

                float3 gridPoint = v.vertex.xyz;
                float3 tangent = float3(1, 0, 0);
                float3 binormal = float3(0, 0, 1);
                float3 p = gridPoint;

                p += GerstnerWaves(waveA, gridPoint, tangent, binormal);
                //p += GerstnerWaves(waveB, gridPoint, tangent, binormal);

                float3 normal = normalize(cross(binormal, tangent));

                v.vertex.xyz = p;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float4 OceanMap = tex2D(_OceanMap, i.uv);

                col = lerp(float4(0, 0, 0, 0), col, OceanMap.a > 0.1f);
                
                clip(col.a - 0.5f);

                return col;
            }
            ENDCG
        }
    }
}
