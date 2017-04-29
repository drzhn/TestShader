Shader "Custom/SnowTrail" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}

		_EdgeLength("Edge length", Range(3,50)) = 10
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
				#pragma surface surf Lambert  addshadow fullforwardshadows  vertex:disp tessellate:tessEdge
				#pragma target 4.6
				#include "Tessellation.cginc"	

				struct appdata {
					float4 vertex : POSITION;
					float4 tangent : TANGENT;
					float3 normal : NORMAL;
					float2 texcoord : TEXCOORD0;
				};

				sampler2D _MainTex;
				float _EdgeLength;

				float4 tessEdge(appdata v0, appdata v1, appdata v2)
				{
					return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
				}

				void disp(inout appdata v)
				{
					float height = tex2Dlod(_MainTex, float4(v.texcoord.xy, 0, 0)).x;
					v.vertex.xyz += v.normal*(height - 1);
				}

				struct Input {
					float2 uv_MainTex;
					float4 screenPos;
					float3 worldPos;
				};
				
				void surf(Input IN, inout SurfaceOutput o) {

					half4 c = tex2D(_MainTex, IN.uv_MainTex);
					o.Albedo = c.rgb;
				}
				inline float4 LightingUnlit(SurfaceOutput s, fixed3 lightDir, fixed atten)
				{
					return fixed4(s.Albedo, s.Alpha);
				}
			ENDCG
		}
			FallBack "Diffuse"
}
