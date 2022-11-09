Shader "Custom3D/MoveSphereTexture" {
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_OverlayTex ("Overlay Texture", 2D) = "red" {}
		[PerRendererData] TimeEnabled("Time Enabled", Float) = 0
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_Speed("Move Speed", Vector) = (0, 1, 0, 0)
		_OverlayScale("Overlay Scale", Vector) = (1, 1, 0, 0)
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
			sampler2D _OverlayTex;
			float _AlphaSplitEnabled;

			float4 _MainTex_TexelSize;
			float _OverlayTex_TexelSize;
			float TimeEnabled;
			float4 _Speed;
			float4 _OverlayScale;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

				// compute overlay texture movement
				fixed4 overlay = tex2D(_OverlayTex, (IN.texcoord * _OverlayScale.xy) + (Time.y - TimeEnabled)*_Speed.xy);

				c.rgb = lerp(c.rgb, overlay.rgb, overlay.a);

				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
