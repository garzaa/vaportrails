Shader "Custom2D/MaskDistortion"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_Mask1 ("Mask1", Color) = (1,1,1,1)
		_Mask2 ("Mask2", Color) = (1,1,1,1)
		_Mask3 ("Mask3", Color) = (1,1,1,1)

		[Header(Background)]
		_BumpAmt   ("Distortion Strength", Range(0, 0.5)) = 0.1
		_LerpMult ("Tend Towards BG", Range(-1, 1)) = 0
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

        GrabPass {
            Tags { "LightMode" = "Always" }
        }

		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UNITYCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				float4 uvgrab : TEXCOORD0;
			};
			
			v2f vert(appdata_t v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvgrab = ComputeGrabScreenPos(o.vertex);
				return o;
			}

			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;
			float4 _MainTex_TexelSize;

			half4 frag(v2f i): COLOR {
				half4 color = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
				return color;
			}

			ENDCG
		}

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
				float4 uvgrab   : TEXCOORD0;
				float2 uvmain   : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
			};
			
			fixed4 _Color;
			
			float _BumpAmt;
			float4 _MainTex_ST;
			v2f vert(appdata_t v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvgrab = ComputeGrabScreenPos(o.vertex);
				

				o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.worldPos = mul (unity_ObjectToWorld, v.vertex);

				o.color = v.color * _Color;

				return o;
			}

			sampler2D _MainTex;
			float4 _Mask1, _Mask2, _Mask3;
			sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;
			float _LerpMult;
			
			float flipIfOdd(float a) {
				return trunc(a*1000) % 2 == 0 ? a : -a;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// get the normal color
				fixed4 c = tex2D(_MainTex, i.uvmain); // SampleSpriteTexture (i.uvmain) * i.color;

				float2 offset = float2(0, 0);
				float lerpAmt = 0;

				// if there's a match with either mask color
                if (any(compareColor(c, _Mask1, 0.1) || compareColor(c, _Mask2, 0.1) || compareColor(c, _Mask3, 0.1))) {
					offset.x = flipIfOdd(c.g);
					offset.y = flipIfOdd(c.b);

					lerpAmt = 0.5 + (_LerpMult * 0.5);
                }

				i.uvgrab.xy += ((offset * _BumpAmt) * i.uvgrab.z);

				// de-blur the image: floor to texel corner, then sample exact center
				// (grabpass is bilinearly filtered)
				i.uvgrab.xy = floor(i.uvgrab.xy / _GrabTexture_TexelSize.xy) * _GrabTexture_TexelSize.xy;
				i.uvgrab.xy += float2(_GrabTexture_TexelSize.x * 0.5, _GrabTexture_TexelSize.y * 0.5);

				// then make sure the sample only falls in the center of texels

				half4 grabPixel = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));

				fixed4 color = lerp(c, grabPixel, lerpAmt);

				color.rgb *= color.a;

				color.a *= i.color.a;

				return color;
			}
		ENDCG
		}
	}
}
