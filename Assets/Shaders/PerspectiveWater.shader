Shader "Custom2D/PerspectiveWater"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_NoiseTex ("Noise Texture", 2D) = "white" {}
	 	_ColorRamp ("Color Ramp", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_MainScale ("Main Scale", Vector) = (1, 1, 0, 0)
		_HorizonDistance ("Horizon Distance", Float) = 4
		_MoveSpeed ("Move Speed", Vector) = (0, 0, 0, 0)
		_DistanceRamp ("Distance Ramp", 2D) = "white" {}
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
				float3 worldpos : float3;
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
			float _HorizonDistance;

			// TODO: JUST USE A GRADIENT YOU STUPID FUCK
			fixed4 SampleSpriteTexture (float2 uv, float3 worldpos) {

				// make uv x centered
				// i.e. map from 0-1 to -1 - 1
				uv.x = (uv.x*2) - 1;
				
				// bottom starts at 0
				uv.y = 1 - uv.y;

				// ok this can't be linear
				// it needs to increase massively and then decrease a bit
				float logCurve = log(uv.y)+1;
				float powerCurve = pow(uv.y, 10);

				// closer UVs use more of the worldPosition
				uv.x = lerp(uv.x, worldpos.x, 1-logCurve);

				//return lerp(float4(1, 0, 0, 1), float4(0, 0.5, 1, 1), 1-logCurve);

				uv.y *= powerCurve*_HorizonDistance;
				// multiplying it by the right value makes a curve
				uv.x /= lerp(0, _HorizonDistance, 1-logCurve);

				uv += _Time.x * _MoveSpeed;

				fixed4 c = tex2D (_NoiseTex, uv/_MainScale.xy);
				// add another moving texture for extra noise
				fixed4 c2 = tex2D(_NoiseTex, (uv+ fixed2(_Time.x/2, _Time.z/2))/_MainScale.xy/4 );

				c = lerp(c, c2, c2.r*0.5);

				// then do the color ramp
				// horizontal: brightness
				// vertical: camera distance
				c = tex2D(_ColorRamp, fixed2(c.r, powerCurve));

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
