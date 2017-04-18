// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Refractive"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Eta("Eta", Range(0,5)) = 1.5
		_BumpAmt("Distortion", range(0,128)) = 10
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }
		LOD 100
		Cull Off
		GrabPass{
		Name "BASE"
		Tags{ "LightMode" = "Always" }
		}

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD1;
				half3 worldNormal : TEXCOORD2;
				half3 worldRefr : TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;
			
			float _BumpAmt;
			float _Eta;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.screenPos = ComputeScreenPos(o.vertex);
				
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldRefr = refract(worldViewDir, o.worldNormal, _Eta);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//fixed4 col = tex2D(_MainTex, i.uv);
				half2 bump = half2(1,0);// i.worldRefr.rg; // we could optimize this by just reading the x & y without reconstructing the Z
				float2 offset = bump *_BumpAmt* _GrabTexture_TexelSize.xy;

				#ifdef UNITY_Z_0_FAR_FROM_CLIPSPACE //to handle recent standard asset package on older version of unity (before 5.5)
				i.screenPos.xy = offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(i.screenPos.z) + i.screenPos.xy;
				#else
				i.screenPos.xy = offset * i.screenPos.z + i.screenPos.xy;
				#endif

				half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.screenPos));
				return col;
			}
			ENDCG
		}
	}
}
