Shader "Custom2D/FlatSky"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
	 	_ColorRamp ("Color Ramp", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_MainScale ("Main Scale", Vector) = (1, 1, 0, 0)
		_NearScale ("Near Scale", Vector) = (0.5, 0.5, 0, 0)
		_FarScale ("Far Scale", Vector) = (5, 5, 0, 0)
		_StrengthMultiplier("StrMultiplier", Float) = 1
		_Offset ("Offset", Vector) = (0, 0, 0, 0)

		_BaseUV("Script UV", Vector) = (0, 0, 0, 0)
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
				float3 worldPos : TEXCOORD1;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif


				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _ColorRamp;
			float4 _NearScale, _FarScale;
			float4 _MainScale, _MoveSpeed, _Offset;
			float _StrengthMultiplier;
			float4 _BaseUV;

			fixed4 SampleSpriteTexture (float2 uv, fixed4 tint, v2f IN)
			{
				float textureYPos = uv.y;

				// make uv x start at 0.5 instead?
				// map uv.x from 0-1 to -1 - 1
				uv.xy *= _MainScale;

				uv.xy *= lerp(_NearScale.xy, _FarScale.xy, textureYPos);
				// return lerp(float4(1, 0, 0, 1), float4(0, 0, 1, 1), textureYPos);

				uv.x = (uv.x*2) - 1;
				uv += _Offset.xy;
				uv += _BaseUV * _StrengthMultiplier * 0.001;

				fixed4 c = tex2D (_MainTex, uv);


				// then do the color ramp
				// tend towards the ramp top based on tint alpha (for dynamically showing/clearing skies);
				float rampPos = c.r * uv.y * tint.a;
				rampPos = lerp(rampPos, 1, 1-tint.a);
				c = tex2D(_ColorRamp, fixed2(rampPos, 0));

				c.rgb *= tint.rgb;

				return c;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 outColor = SampleSpriteTexture (IN.texcoord, IN.color, IN);
				outColor.rgb *= outColor.a;
				return outColor;
			}
		ENDCG
		}
	}
}
