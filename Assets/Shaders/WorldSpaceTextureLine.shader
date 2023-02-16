Shader "Custom2D/WorldSpaceTextureLine"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[Header(period speed amplitude onramp)]_WaveParams("Wave Params", Vector) = (1, 1, 1, 1)
		_TextureSpeed("Texture Speed", Float) = 1
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
			fixed4 _WaveParams;

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
            float4 _MainTex_ST;
			float _TextureSpeed;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				float offset = sin(uv.x / _WaveParams.x + (_Time.x * _WaveParams.y)) * _WaveParams.z;
				offset *= saturate(lerp(0, 1, uv.x / _WaveParams.w));
				uv.y += offset;
				uv.x += _TextureSpeed * _Time.x;
				fixed4 color = tex2D (_MainTex, uv);
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{

				float2 uv = IN.texcoord;
				uv.x *= _MainTex_ST.x;

				fixed4 c = SampleSpriteTexture (uv) * IN.color;
				c.rgb *= c.a;

				return c;
			}
		ENDCG
		}
	}
}