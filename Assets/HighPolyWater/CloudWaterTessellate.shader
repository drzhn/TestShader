Shader "Custom/CloudWaterTessellate" {
	Properties{
		_TopColor("Top Color", Color) = (1,1,1,1)
		_BottomColor("Bottom Color", Color) = (1,1,1,1)
		_NoiseTex("Noise", 2D) = "white" {}

		_EdgeLength("Edge length", Range(3,50)) = 10
	}
		SubShader{
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
			LOD 200
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma surface surf Unlit alpha vertex:disp tessellate:tessEdge
			#pragma target 3.0
			#include "Tessellation.cginc"

			sampler2D _NoiseTex;

			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct Input {
				float2 uv_NoiseTex;
				float4 screenPos;
				float3 worldPos;
			};

			fixed4 _TopColor;
			fixed4 _BottomColor;
			sampler2D_float _CameraDepthTexture;
			float _EdgeLength;

			float4 tessEdge(appdata v0, appdata v1, appdata v2)
			{
				return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
			}

			void disp(inout appdata v)
			{
				float noise = tex2Dlod(_NoiseTex, float4(v.uv.xy, 0, 0)).x;
				v.vertex.xyz += v.normal*((sin(pow(v.uv.x - 0.2, 2) * 10 + pow(v.uv.y - 0.2, 2) * 15 - _Time.z / 2) +
					(sin(pow(v.uv.x - 0.7, 2) * 15 + pow(v.uv.y - 0.8, 2) * 10 - _Time.z / 3)))*0.25 + 0.5)*noise*1.5;
			}

			void surf(Input IN, inout SurfaceOutput o) {
				// Albedo comes from a texture tinted by color
				//fixed4 c = tex2D (_NoiseTex, IN.uv_NoiseTex) * _TopColor;
				float3 localPos = IN.worldPos - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
				o.Albedo = lerp(_BottomColor, _TopColor, localPos.y);

				half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
				depth = LinearEyeDepth(depth);
				half diff = saturate(3 * (depth - IN.screenPos.w));
				//col.a = lerp(0, 1, diff);
				o.Alpha = lerp(0, 1, diff);// 0.1;// c.a;
			}
			inline float4 LightingUnlit(SurfaceOutput s, fixed3 lightDir, fixed atten)
			{
				return fixed4(s.Albedo, s.Alpha);
			}

			ENDCG
		}
			FallBack "Diffuse"
}
