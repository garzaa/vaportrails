Shader "Custom2D/CircleWrap"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
        _Mask1 ("Mask1", Color) = (1,1,1,1)
        _Mask2 ("Mask2", Color) = (1,1,1,1)
        _Overlay ("Overlay Texture", 2D) = "white" {}
        _Speed ("Move Speed", Vector) = (1, 1, 0, 0)
		_AlphaRamp ("Alpha Ramp", 2D) = "white" {}
		_AlphaTexture ("Alpha Texture", 2D) = "white" {}
        _AlphaTextureSpeed ("Alpha Texture Speed", Vector) = (0, 0, 0, 0)
		_AlphaCutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		_RadialScale("Radial Scale", Float) = 1.0
        _LengthScale("Length Scale", Float) = 1.0
		_OverlayBoost("Overlay Boost", Float) = 0.0
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
            sampler2D _Overlay;
			sampler2D _AlphaRamp, _AlphaTexture;
            float4 _MainTex_TexelSize;
            float4 _Overlay_TexelSize;
			float4 _Overlay_ST;
            fixed4 _Mask1;
            fixed4 _Mask2;
            float4 _Speed, _AlphaTextureSpeed;
			float4 _AlphaTexture_ST, _AlphaRamp_ST;
			float _AlphaCutoff;
			float _RadialScale;
            float _LengthScale;
			float _OverlayBoost;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 uv = IN.texcoord;
                Unity_PolarCoordinates_float(uv, float2(0.5, 0.5), _RadialScale, _LengthScale, uv);
				// get the normal color
				fixed4 c = SampleSpriteTexture (uv) * IN.color;

				uv.x += _Speed.x * _Time.x;
				uv.y += _Speed.y * _Time.x;

                // get the mask color
                // it's gonna be a different size texture, so normalize to pixels for uv lookup
                fixed2 maskUV = (uv / _MainTex_TexelSize) * _Overlay_TexelSize;
                fixed4 overlay = tex2D (_Overlay, uv + (_Time.w * _Speed) * _Overlay_ST.xy);

                // if there's a match with either mask color
                if (any(compareColor(c, _Mask1, 0.1) || compareColor(c, _Mask2, 0.1))) {
                    c.rgb = lerp(c.rgb, overlay.rgb, overlay.a);
					c.a = lerp(c.a, overlay.a, overlay.a + _OverlayBoost);
                }

				// TODO: then overlay with the alpha ramp and the alpha texture
				// if below 50%, clip it?
				float ramp = tex2D(_AlphaRamp, IN.texcoord);
				float tex = tex2D(_AlphaTexture, IN.texcoord + (_Time.w * _AlphaTextureSpeed));

				clip(ramp*tex - _AlphaCutoff);

				c.rgb *= c.a;


				return c;
			}
		ENDCG
		}
	}
}
