Shader "Custom2D/MovingTexture"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
        _Speed ("Move Speed", Vector) = (1, 1, 0, 0)
		_DistortTex ("Distortion Texture", 2D) = "white" {}
		_RadialScale("Radial Scale", Float) = 1.0
        _LengthScale("Length Scale", Float) = 1.0
		_DistortStrength("Distort Strength", Range(0.0, 1)) = 0
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

		Lighting Off
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
			sampler2D _DistortTex;
			float4 _DistortTex_ST;
            float4 _Speed;
			float _RadialScale;
            float _LengthScale;
			float _DistortStrength;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target {
				// get the distortion: Two floats
				float2 radialUV = IN.texcoord;
				// convert to radial
				Unity_PolarCoordinates_float(radialUV, float2(0.5, 0.5), _RadialScale, _LengthScale, radialUV);
				fixed2 noise = tex2D(_DistortTex, radialUV+ _Speed.xy*_Time.w).rg;
				// normalize from 0, 1 to -1, 1
				noise = fixed2(-1, -1) + (noise*2);

				IN.texcoord += noise * _DistortStrength;

				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c.rgb *= c.a;

				return c;
			}
		ENDCG
		}
	}
}
