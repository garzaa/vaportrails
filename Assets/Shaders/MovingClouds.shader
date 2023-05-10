Shader "Custom2D/MovingClouds"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_NoiseTex ("Noise Texture", 2D) = "white" {}
	 	_ColorRamp ("Color Ramp", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_MainScale ("Main Scale", Vector) = (1, 1, 0, 0)
		_MoveSpeed ("Move Speed", Vector) = (0, 0, 0, 0)
		_TextureMoveSpeed ("Texture Change Speed", Vector) = (1, 1, 0, 0)
		_AlphaAdd("Add Alpha", Range(0.0, 1.0)) = 0.0
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
				float2 texcoord : TEXCOORD0;
				float3 worldpos : TEXCOORD1;
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
				OUT.worldpos = mul(unity_ObjectToWorld, IN.vertex);

				return OUT;
			}

			sampler2D _MainTex, _NoiseTex;
			float4 _NoiseTex_TexelSize;
			sampler2D _ColorRamp;
			float4 _NearScale, _FarScale;
			float4 _MainScale, _MoveSpeed;
			fixed4 _TextureMoveSpeed;
			float _AlphaAdd;

			fixed4 SampleSpriteTexture (float2 uv, float3 worldpos) {
				uv += _Time.x * _MoveSpeed;

				fixed4 c1 = tex2D (_NoiseTex, uv/_MainScale.xy);
				// add another moving texture for extra noise
				fixed4 c2 = tex2D(_NoiseTex, (uv+ fixed2(_Time.x/2 * _TextureMoveSpeed.x, _Time.z/2 * _TextureMoveSpeed.y))/_MainScale.xy/4 );

				fixed4 c = lerp(c1, c2, c2.r*0.5);

				// then do the color ramp
				// horizontal: brightness
				// vertical: camera distance
				c = tex2D(_ColorRamp, fixed2(c.r, uv.y));
				
				c.a = saturate(c.a + _AlphaAdd);

				return c;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord, IN.worldpos) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
