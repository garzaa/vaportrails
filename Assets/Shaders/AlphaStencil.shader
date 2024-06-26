Shader "Custom2D/AlphaStencil"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.0

		_DissolveTex ("Dissolve Texture", 2D) = "white" {}

		[Header(Stencil)]
		_Stencil ("Ref Val [0;255]", Float) = 0
		_ReadMask ("ReadMask [0;255]", Int) = 255
		_WriteMask ("WriteMask [0;255]", Int) = 255
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Draw Comparison (Draw If Ref <?> Buffer)", Int) = 3
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Buffer Operation (if comparison success)", Int) = 0
		// [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
		// [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0
	}

	SubShader
	{
		Stencil
		{
			Ref [_Stencil]
			ReadMask [_ReadMask]
			WriteMask [_WriteMask]
			Comp [_StencilComp]
			Pass [_StencilOp]
			// Fail [_StencilFail]
			// ZFail [_StencilZFail]
		}

		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _DissolveTex;
			float4 _DissolveTex_ST;
			float _AlphaCutoff;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D (_MainTex, IN.texcoord);

				// then multiply by the dissolve texture lookup
				float2 d_uv = IN.texcoord.xy * _DissolveTex_ST.xy + (_DissolveTex_ST.zw * _Time.x);
				fixed d = tex2D(_DissolveTex, d_uv).a;

				c.a *= d;

				c *= IN.color;

				if (c.a < _AlphaCutoff) {
					discard;
				}

				c.a = 1;

				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
