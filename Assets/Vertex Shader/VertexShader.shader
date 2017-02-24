Shader "Custom/VertexShader" {
	Properties {
		_MainTint("Global Color Tint", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#include "UnityCG.cginc"

		float4 _MainTint;

		struct Input {
			//float2 uv_MainTex;
			float4 vertColor;
		};

		void vert(inout appdata_full v, out Input o)
		{
			o.vertColor = (v.vertex);
		}


		void surf(Input IN, inout SurfaceOutput o)
		{
			o.Albedo = IN.vertColor.rgb * _MainTint.rgb;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
