Shader "Unlit/SingleColor"
{
	Properties
	{
		// Color property for material inspector, default to white
		//_Color("Main Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Pass
		{
			Name "OUTLINE"
			//Tags{ "LightMode" = "Always" }
			//Cull Off
			ZWrite On
			ZTest Greater
			//Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};
			struct v2f
			{
				float4 vertex : SV_POSITION;
			};
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex =  mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}

			// pixel shader, no inputs needed
			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(1,0,0,1); // just return it
			}
			ENDCG
		}
		Pass
		{
			Name "BASE"
			//Tags{ "LightMode" = "Always" }
			ZWrite On
			ZTest Less

			//Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};
			struct v2f
			{
				float4 vertex : SV_POSITION;
			};
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex =  mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}


			// pixel shader, no inputs needed
			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(1,1,1,1); // just return it
			}
			ENDCG
		}
	}
}