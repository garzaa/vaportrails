Shader "Custom2D/ColorReplace"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_Mask1 ("Mask1", Color) = (1,1,1,1)
		_Mask2 ("Mask2", Color) = (1,1,1,1)
		_Mask3 ("Mask3", Color) = (1,1,1,1)
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
				float2 uvmain   : TEXCOORD2;
			};
			
			fixed4 _Color;
			
			float4 _MainTex_ST;
			v2f vert(appdata_t v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				

				o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color * _Color;

				return o;
			}

			sampler2D _MainTex;
			float4 _Mask1, _Mask2, _Mask3;
			
			float flipIfOdd(float a) {
				return trunc(a*1000) % 2 == 0 ? a : -a;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// get the normal color
				fixed4 c = tex2D(_MainTex, i.uvmain); // SampleSpriteTexture (i.uvmain) * i.color;

				// if there's a match with either mask color
                if (any(compareColor(c, _Mask1, 0.1) || compareColor(c, _Mask2, 0.1) || compareColor(c, _Mask3, 0.1))) {
					c = _Color;
                }

				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
