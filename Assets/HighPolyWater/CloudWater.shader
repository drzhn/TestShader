Shader "Custom/CloudWater"
{
	Properties
	{
		_BottomColor("Bottom Color",Color) = (0,0,1,1)
		_TopColor("Top Color",Color) = (0,0,1,1)
		_NoiseTex("Noise",2D) = "white" {}
	}
		SubShader
		{
			Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
			LOD 100

			Pass
			{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			//Cull Off
			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertexClip : SV_POSITION;
				float4 screenPos : TEXCOORD1;
				float4 vertexModel : TEXCOORD2;
			};

			sampler2D _NoiseTex;
			sampler2D_float _CameraDepthTexture;
			half4 _BottomColor;
			half4 _TopColor;

			v2f vert(appdata v)
			{
				v2f o;
				float noise = tex2Dlod(_NoiseTex,float4(v.uv,0,0)).x;
				v.vertex.xyz += v.normal*((sin(pow(v.uv.x - 0.2, 2) * 10 + pow(v.uv.y - 0.2, 2) * 15 - _Time.z / 2) +
					(sin(pow(v.uv.x - 0.7, 2) * 15 + pow(v.uv.y - 0.8, 2) * 10 - _Time.z / 3)))*0.25 + 0.5)*noise*1.5;

				o.vertexModel = v.vertex;
				o.vertexClip = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.screenPos = ComputeScreenPos(o.vertexClip);
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{

				half4 col = lerp(_BottomColor,_TopColor,i.vertexModel.y);

				//Intersection transparency
				half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
				depth = LinearEyeDepth(depth);
				half diff = saturate(3 * (depth - i.screenPos.w));
				col.a = lerp(0, 1,diff);
				return col;
			}
			ENDCG
			}
		}
	Fallback "Transparent/Diffuse"
}
