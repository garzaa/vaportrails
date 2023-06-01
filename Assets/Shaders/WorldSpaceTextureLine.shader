Shader "Custom2D/WorldSpaceTextureLine"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)

		[Header(period speed amplitude onramp)]_WaveParams("Wave Params", Vector) = (1, 1, 1, 1)
		_TextureSpeed("Texture Speed", Float) = 1
		_AmpWave("Amplitude Wave", Range(0, 1)) = 0

		[Header(Alpha)]
		_AlphaRamp ("Alpha Ramp ", 2D) = "white" {}
		_AlphaCutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.0

		[Header(Stencil)]
		_Stencil ("Ref Val [0;255]", Float) = 0
		_ReadMask ("ReadMask [0;255]", Int) = 255
		_WriteMask ("WriteMask [0;255]", Int) = 255
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Draw Comparison (Draw If Ref <?> Buffer)", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Buffer Operation (if comparison success)", Int) = 0
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

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
            #include "Assets/Shaders/utils.cginc"
			
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
				float3 worldPos : TEXCOORD1;
			};
			
			fixed4 _Color;
			fixed4 _WaveParams;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);

				return OUT;
			}

			sampler2D _MainTex;
            float4 _MainTex_ST;
			float _TextureSpeed;
			float _AmpWave;

			sampler2D _AlphaRamp;
			float _AlphaCutoff;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				// sample the alpha ramp before fucking with the UVs
				float a = tex2D(_AlphaRamp, uv).a;

				float offset = sin(uv.x / _WaveParams.x + (_Time.x * _WaveParams.y)) * _WaveParams.z;
				offset *= saturate(lerp(0, 1, uv.x / _WaveParams.w));

				// move slider to the right to pinch the ends of the waves
				offset *= lerp(1, sin(uv.x * 3.14159265f), _AmpWave);

				uv.y += offset;
				uv.x += _TextureSpeed * _Time.x;
				fixed4 color = tex2D (_MainTex, uv);

				// if alpharamp.a * color.a < cutoff, clip
				clip((a * color.a) - _AlphaCutoff);

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{

				float2 uv = IN.texcoord;
				uv.x *= _MainTex_ST.x;

				fixed4 c = SampleSpriteTexture (uv) * IN.color;
				c.rgb *= c.a;

				return c;
			}
		ENDCG
		}
	}
}
