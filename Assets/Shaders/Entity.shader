Shader "Custom2D/Entity"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_NoiseTex("Noise Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] whiteFlashTime ("whiteFlashTime", Float) = -100
		[PerRendererData] cyanFlashTime ("cyanFlashTime", Float) = -100
		[PerRendererData] flinchWeight ("flinchWeight", Float) = 0
		[PerRendererData] flinchDirection ("flinchDirection", Vector) = (0, 0, 0, 0)
		[PerRendererData] whiteFlashWeight ("whiteFlashWeight", Float) = 0
		[PerRendererData] transparency ("transparency", Float) = 0
		[PerRendererData] noWaveTime ("noWaveTime", Float) = -100
		[PerRendererData] _NoWaveAmt ("_NoWaveAmt", Range(0.0, 1.0)) = 0
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
				float3 worldPos : TEXCOORD1;
			};
			
			fixed4 _Color;

			float flinchWeight;
			float4 flinchDirection;
			float _UnscaledTime;
			float _NoWaveAmt;
			float noWaveTime;
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;

			float4 flinchVertex(float4 vert) {
				float4 target = vert + flinchDirection*flinchWeight*0.05;
				return lerp(vert, target, sin(_Time.y * 100));
			}

			float noWaveStr() {
				// say it takes 3 seconds, get the fraction of time and multiply by no wave amt
				return (((_UnscaledTime - noWaveTime) / 4.5) * _NoWaveAmt);
			}

			float4 wobbleVertex(float4 vert) {
				float2 worldPos = mul(unity_ObjectToWorld, vert).xy;
				float2 noiseUV1 = worldPos * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
				float2 noiseUV2 = worldPos * _NoiseTex_ST.xy + (_NoiseTex_ST.zw * _Time.x);

				// need to use tex2dlod with manual mipmap sampling
				fixed4 noise1 = tex2Dlod(_NoiseTex, float4(noiseUV1.x, noiseUV1.y, 0, 0));
				fixed4 noise2 = tex2Dlod(_NoiseTex, float4(noiseUV2.x, noiseUV2.y, 0, 0));
				fixed4 offset = lerp(noise1, noise2, noise1.b);
				// normalize to -1, 1
				offset = (offset * 2) - 1;
				offset *= noWaveStr();
				offset *= 8;
				vert.xy += offset;
				return vert;
			}

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				IN.vertex = wobbleVertex(IN.vertex);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				OUT.vertex = flinchVertex(OUT.vertex);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif
				OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
				return OUT;
			}

			sampler2D _MainTex;
			float _AlphaSplitEnabled;
			float whiteFlashTime;
			float cyanFlashTime;
			float whiteFlashWeight;
			float transparency;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
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

			void NoWave(fixed4 c, v2f IN) {
				// displace vertices on two overlaid moving noise textures
				// and dissolve on one noise texture
				float2 worldPos = IN.worldPos.xy;
				float2 uv = IN.texcoord;
				fixed4 noise = tex2D(_NoiseTex, IN.worldPos/4);
				clip(noise.r - noWaveStr() - 0.2);
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c = WhiteFlash(c);
				c = CyanFlash(c);
				c = ContinuousWhiteFlash(c);
				NoWave(c, IN);
				c.a *= (1 - transparency);
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
