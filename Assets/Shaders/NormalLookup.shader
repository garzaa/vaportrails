Shader "Custom3D/NormalLookup" {

	Properties {
		_ColorMap ("Sprite Texture", 2D) = "white" {}
		_NoiseStr ("Noise Strength", Range(0.0, 1.0)) = 0
		_NoiseTex ("Noise Texture", 2D) = "white" {}
	}

    SubShader {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float3 normal : TEXCOORD0;
				half2 texcoord  : TEXCOORD1;
			};


			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.normal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

            sampler2D _ColorMap;
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			float _NoiseStr;
        
            fixed4 frag (v2f i) : SV_Target {
				half4 color;
				// now just offset normal.xy by the noise texture's r and b values
				half2 noiseOffset = (tex2D(_NoiseTex, i.texcoord.xy * _NoiseTex_ST.xy).rb);
				// convert to -1, 1
				noiseOffset = noiseOffset * 2 - 1;
				// scale by noiseStr
				noiseOffset *= _NoiseStr;
				color = tex2D(_ColorMap, ((i.normal.xy + 1) / 2) + noiseOffset);
				return color;
				return tex2D(_NoiseTex, i.texcoord.xy * _NoiseTex_ST * _NoiseStr);
            }
            ENDCG
        }
    }
}
