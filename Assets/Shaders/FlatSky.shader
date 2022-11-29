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
		_MoveSpeed ("Move Speed", Vector) = (0, 0, 0, 0)
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
			sampler2D _ColorRamp;
			float4 _NearScale, _FarScale;
			float4 _MainScale, _MoveSpeed;

			fixed4 SampleSpriteTexture (float2 uv, fixed4 fadeColor)
			{
				float textureYPos = uv.y;

				// make uv x start at 0.5 instead?
				// map uv.x from 0-1 to -1 - 1
				uv.x = (uv.x*2) - 1;

				uv.xy *= _MainScale;

				uv.xy *= lerp(_NearScale.xy, _FarScale.xy, textureYPos);
				// return lerp(float4(1, 0, 0, 1), float4(0, 0, 1, 1), textureYPos);

				uv += _Time.x * _MoveSpeed;
				fixed4 c = tex2D (_MainTex, uv);

				// then do the color ramp
				c = tex2D(_ColorRamp, fixed2(c.r * uv.y, 0));

				c.rgb = lerp(c.rgb, fadeColor.rgb, textureYPos * fadeColor.a);

				return c;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord, IN.color);
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
