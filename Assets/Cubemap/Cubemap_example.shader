Shader "Custom/Cubemap_example" {
	Properties{
		_MainTint("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Cubemap("Cubemap",CUBE) = ""{}
		_ReflTexture("Reflection texture", 2D) = "white"{}
		_ReflAmount("Reflection Amount", Range(0.01, 1)) = 0.5
		_NormalMap("Normal Map", 2D) = "bump" {}

	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		samplerCUBE _Cubemap;
		sampler2D _ReflTexture;
		float _ReflAmount;
		float4 _MainTint;
		sampler2D _NormalMap;



		struct Input {
			float2 uv_MainTex;
			float2 uv_ReflTexture;
			float3 worldRefl;
			float2 uv_NormalMap;
			INTERNAL_DATA
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			half4 c = tex2D(_MainTex, IN.uv_MainTex) * _MainTint;
			float3 normals = float3(0, 0,-1);//= UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap)).rgb;
			

			float amount = tex2D(_ReflTexture, IN.uv_ReflTexture).r;
			o.Emission = texCUBE(_Cubemap, WorldReflectionVector(IN,o.Normal)).rgb;
			//o.Normal = normals;
			o.Albedo = c.rgb;
			o.Alpha = c.a;

		}
		ENDCG
	}
		FallBack "Diffuse"
}
