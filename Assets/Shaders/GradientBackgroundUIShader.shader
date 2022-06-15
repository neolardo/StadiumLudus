Shader "Unlit/GradientBackgroundUIShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0,0,0,0)
        _Direction("Direction", Integer) = 0
        _Offset("Offset", Float) = 0
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            int _Direction;
            float _Offset;
            float _BaseAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float dist(float2 a, float2 b)
            {
                return sqrt(((a.x - b.x) * (a.x - b.x)) + ((a.y - b.y) * (a.y - b.y)));
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 baseColor = float4(_Color.x, _Color.y, _Color.z, _Color.w);
                if (_Direction == 0)
                {
                    return  i.uv.x > (1-_Offset) ? baseColor : baseColor * float4(1,1,1, (i.uv.x / (1-_Offset)));
                }
                else if (_Direction == 1)
                {
                    return float4(_Color.x, _Color.y, _Color.z, i.uv.y* i.uv.y);
                }
                else if (_Direction == 2)
                {
                    return  i.uv.x < _Offset ? baseColor : baseColor * float4(1, 1, 1, 1- ((i.uv.x -_Offset) / (1-_Offset)));
                }
                else if (_Direction == 3)
                {
                    return float4(_Color.x, _Color.y, _Color.z, (1 - i.uv.y)*(1 - i.uv.y));
                }
                else return 0;
            }
            ENDCG
        }
    }
}
