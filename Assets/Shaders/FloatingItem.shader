Shader "Custom2D/FloatingItem"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
        _Overlay ("Overlay Texture", 2D) = "white" {}
        _Speed ("Move Speed", Vector) = (1, 1, 0, 0)
		_Outline("Outline", Float) = 0
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineSize("Outline Size", int) = 1
		_Rect("Rect Display", Vector) = (0,0,1,1)
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
				OUT.vertex.y += sin(_Time.z) * 0.01;
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);

				return OUT;
			}

			sampler2D _MainTex;
            sampler2D _Overlay;
            float4 _MainTex_TexelSize;
            float4 _Overlay_TexelSize;
            float4 _Speed;
			float _Outline;
			float4 _OutlineColor;
			int _OutlineSize;
			float4 _Rect;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
                // get the normal color
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;

                // get the mask color
				fixed2 maskUV = (IN.texcoord / _MainTex_TexelSize) * _Overlay_TexelSize;
                fixed4 overlay = tex2D (_Overlay, maskUV + (_Time.w * _Speed));

				c.rgb = lerp(c.rgb, overlay.rgb, overlay.a);
				
				c.rgb *= c.a;

				// outline stuff
				if (_Outline > 0 && c.a != 0) {
					float totalAlpha = 1.0;

					if (IN.texcoord.x < _Rect.x + _MainTex_TexelSize.x || IN.texcoord.y < _Rect.y + _MainTex_TexelSize.y ||
						IN.texcoord.x > _Rect.z - _MainTex_TexelSize.x || IN.texcoord.y > _Rect.w - _MainTex_TexelSize.y)
					{
						totalAlpha = 0;
					}
					else
					{
						[unroll(16)]
						for (int i = 1; i < _OutlineSize + 1; i++) {
							fixed4 pixelUp = tex2D(_MainTex, IN.texcoord + fixed2(0, i * _MainTex_TexelSize.y));
							fixed4 pixelDown = tex2D(_MainTex, IN.texcoord - fixed2(0, i *  _MainTex_TexelSize.y));
							fixed4 pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(i * _MainTex_TexelSize.x, 0));
							fixed4 pixelLeft = tex2D(_MainTex, IN.texcoord - fixed2(i * _MainTex_TexelSize.x, 0));

							totalAlpha = totalAlpha * pixelUp.a * pixelDown.a * pixelRight.a * pixelLeft.a;
						}
					}

					if (totalAlpha == 0) {
						c.rgba = fixed4(1, 1, 1, 1) * _OutlineColor;
					}
				}

				return c;
			}
		ENDCG
		}
	}
}
