Shader "Custom/MapShader"
{
    Properties
    {
        _Terrain ("Terrain Map", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "TerrianCompatile" = "True"}
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

            sampler2D _Terrain;
            float4 _Terrain_ST;

            sampler2D _NormalMap;
            float4 _NormalMap_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Terrain);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 Terrain = tex2D(_Terrain, i.uv);
                float4 Normal = tex2D(_NormalMap, i.uv);

                float3 SunDir = _WorldSpaceLightPos0.xyz;
                float3 shading = saturate(saturate(dot(Normal, SunDir) + 0.05)) * 1.5;

                return float4(Terrain.xyz * shading, 1);
            }
            ENDCG
        }
    }
}
