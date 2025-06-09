Shader "Custom/ShadowPlane"
{
    Properties
    {
        _Color ("Shadow Color", Color) = (0,0,0,0.5)
        _FadeDistance ("Fade Distance", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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

            fixed4 _Color;
            float _FadeDistance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Fade out at edges
                float fade = 1 - smoothstep(1-_FadeDistance, 1, length(i.uv - 0.5) * 2);
                return fixed4(_Color.rgb, _Color.a * fade);
            }
            ENDCG
        }
    }
}
