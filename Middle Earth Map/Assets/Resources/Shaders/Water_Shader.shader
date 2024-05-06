Shader "Custom/Water_Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "TerrainCompatible" = "True" }
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

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
            appdata GerstnerWaves(appdata v)
            {
                float3 p = v.vertex.xyz;

                float Steepness = 4.0f;

                float k = 2 * UNITY_PI / 20;
                float a = Steepness / k;
                float c = sqrt(9.8 / k);

                float2 dir = normalize(float2(1, 1));

                float f = k * (dot(dir, p.xz) - c * _Time.y);

                //p.x += a * cos(f);
                //p.y = a * sin(f);
                p.x += dir.x * (a * cos(f));
                p.y = a * sin(f);
                p.z += dir.y * (a * cos(f));

                float3 tangent = normalize(float3(1 - k * Steepness * sin(f), k * Steepness * cos(f), 0));
                float3 normal = float3(-tangent.y, tangent.x, 0);
                
                v.vertex.xyz = p;
                //v.normal = normal;

                return v;
            }

            v2f vert (appdata v)
            {
                v2f o;

                v = GerstnerWaves(v);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
