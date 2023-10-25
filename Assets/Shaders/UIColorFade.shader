Shader "CustomUI/UIColorFade"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Stencil ("Ref Val (0-255)", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Draw Comparison (Draw If Ref <?> Buffer)", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Buffer Operation (if comparison success)", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		_Speed ("Move Speed", Vector) = (0, 0, 0, 0)
        _Offset ("Position Texture Offset", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
			fixed4 _ColorFade;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
			float4 _MainTex_TexelSize;
            float4 _MainTex_ST;

            uniform float4 _Offset;
			float4 _Speed;
            float _UnscaledTime;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // move pixel-perfect
                // texelsize.z: width
                uv.x =((uv.x + (_UnscaledTime * _Speed.x)) * _MainTex_TexelSize.z) / _MainTex_TexelSize.z;
                uv.y =((uv.y + (_UnscaledTime * _Speed.y)) * _MainTex_TexelSize.w) / _MainTex_TexelSize.w;

                uv.x += _Offset.x;
                uv.y += _Offset.y;

                half4 color = tex2D(_MainTex, uv);
				
				if (IN.color.a < 0.5) {
					IN.color.a = 0;
				}

				color.rgb = lerp(color.rgb, IN.color.rgb, IN.color.a);

                return color;
            }
        ENDCG
        }
    }
}
