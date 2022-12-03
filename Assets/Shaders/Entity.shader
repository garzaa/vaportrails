Shader "Custom2D/Entity"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] whiteFlashTime ("whiteFlashTime", Float) = -100
		[PerRendererData] cyanFlashTime ("cyanFlashTime", Float) = -100
		[PerRendererData] flinchWeight ("flinchWeight", Float) = 0
		[PerRendererData] flinchDirection ("flinchDirection", Vector) = (0, 0, 0, 0)
		[PerRendererData] whiteFlashWeight ("whiteFlashWeight", Float) = 0
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
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;

			float flinchWeight;
			float4 flinchDirection;
			float _UnscaledTime;

			float4 flinchVertex(float4 vert) {
				float4 target = vert + flinchDirection*flinchWeight*0.05;
				return lerp(vert, target, sin(_Time.y * 100));
			}

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				OUT.vertex = flinchVertex(OUT.vertex);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			float whiteFlashTime;
			float cyanFlashTime;
			float whiteFlashWeight;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 WhiteFlash(fixed4 c) {
				c.rgb = lerp(fixed3(1, 1, 1), c.rgb, saturate((_UnscaledTime - whiteFlashTime) * 5));
				return c;
			}

			fixed4 CyanFlash(fixed4 c) {
				c.rgb = lerp(fixed3(0, 1, 1), c.rgb, saturate((_UnscaledTime - cyanFlashTime) * 5));
				return c;
			}

			fixed4 ContinuousWhiteFlash(fixed4 c) {
				c.rgb = lerp(c.rgb, fixed3(1, 1, 1), saturate(sin(_Time.w*20)*0.5*whiteFlashWeight));
				return c;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c = WhiteFlash(c);
				c = CyanFlash(c);
				c = ContinuousWhiteFlash(c);
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
