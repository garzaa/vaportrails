Shader "Custom2D/RippleEffect"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		
		_Spread ("Spread", Range(0, 1)) = 0.5
		_Width ("Width", Range(0, 1)) = 0.5
		_DistortStrength   ("Distortion Strength", Range(0, 0.5)) = 0.1
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
			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;
			float _DistortStrength;
			float _Spread;
			float _Width;

			fixed4 frag(v2f i) : SV_Target
			{
				float2 center = float2(0.5, 0.5);

				float outer_map = 1.0 - smoothstep(
					_Spread - _Width,
					_Spread,
					length(i.uvmain.xy - center)
				);

				float inner_map = smoothstep(
					_Spread - _Width * 2.0,
					_Spread - _Width,
					length(i.uvmain.xy - center)
				);
				
				float map = outer_map * inner_map;
				
				float2 displacement = normalize(i.uvmain.xy - center) * _DistortStrength * map;

				i.uvgrab.xy -= displacement;
				half4 color = tex2Dproj(_GrabTexture, i.uvgrab);
				color.rgb *= color.a;
				return color;
			}
		ENDCG
		}
	}
}
