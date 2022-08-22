// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
Shader "CustomUI/ResourceBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_OverlayTex ("Overlay Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		_StartColor ("Start Color", Color) = (1,1,1,1)
		_EndColor ("End Color", Color) = (1,1,1,1)
        _Speed ("OverlaySpeed", Vector) = (1, 0, 0, 0)
        _Direction ("Direction", Vector) = (1, 0, 0, 0)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
            sampler2D _OverlayTex;
            float4 _MainTex_TexelSize;
            float4 _OverlayTex_TexelSize;
            fixed4 _Color;
            fixed4 _EndColor;
            fixed4 _StartColor;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            half4 _Speed;
            half4 _Direction;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                // sample overlay
                // it's gonna be a different size texture, so normalize to pixels for uv lookup
                fixed2 overlayUV = IN.texcoord * _MainTex_ST;
                half4 overlay = tex2D(_OverlayTex, IN.texcoord + (_Speed.xy * _Time.y));
                // get lerp, project current texcoord on the direction
                float t = dot(IN.texcoord / _MainTex_ST, _Direction.xy);
                overlay.rgb = overlay.rgb * lerp(_StartColor, _EndColor, t);
                overlay.a = lerp(overlay.a, 0, t);
                
                // then apply overlay
                color.rgb = lerp(color.rgb, overlay.rgb, overlay.a);

                return color;
            }
        ENDCG
        }
    }
}
