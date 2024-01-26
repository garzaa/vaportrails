// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom2D/MirrorFloor"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[HideInInspector] _ReflectionTex ("", 2D) = "white" {}
		_DistortAmount ("DistortionAmount", Float) = 0.5
	}
	SubShader
	{
		Tags  { 
			"Queue"="Transparent"
			"RenderType"="Transparent" 
		}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float2 uv : TEXCOORD0;
				float4 refl : TEXCOORD1;
				float4 pos : POSITION;
				float4 col: COLOR;
			};
			struct v2f
			{
				half2 uv : TEXCOORD0;
				float4 refl : TEXCOORD1;
				float4 pos : SV_POSITION;
				fixed4 col: COLOR;
			};
			float4 _MainTex_ST;
			fixed4 _Color;
			v2f vert(appdata_t i)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (i.pos);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				o.col = i.col;// * _Color;
				o.refl = ComputeScreenPos (o.pos);
				return o;
			}

			sampler2D _MainTex;
			sampler2D _ReflectionTex;
			float4 _MainTex_TexelSize;
			float _DistortAmount;

			fixed4 SineDisplace(sampler2D _reflTex, float2 uv, v2f IN)
			{
				// poor man's Fresnel effect
				float normY  = -(uv.y - _MainTex_TexelSize);
				// distort more towards the bottom of screen
				uv.x += sin(normY * 500) * 0.0001 * _DistortAmount * (1-pow(IN.uv.y, 2));
				fixed4 color = tex2D (_reflTex, uv) * IN.col;
				fixed4 fade = _Color;
				return lerp(color, fade, saturate(fade.a * (1-IN.uv.y)*3));
				// color.rgb = lerp(color.rgb, _Color, normY / _MainTex_TexelSize);
				// color.rgb *= color.a;
				return color;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// fixed4 tex = tex2D(_MainTex, i.uv) * i.col;
				// tex.rgb *= tex.a;
				fixed4 refl = SineDisplace(_ReflectionTex, UNITY_PROJ_COORD(i.refl), i);
				return refl;
				// fixed4 final = tex * refl;
				// return final;
			}
			ENDCG
	    }

	}
}
