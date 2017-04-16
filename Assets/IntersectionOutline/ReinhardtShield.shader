Shader "Custom/ReinhardtShield"
{
	Properties
	{
		_MainTex("Shield Edge", 2D) = "white" {}
		_MainTint("Sheld Color",Color) = (0,0,1,1)
		_MainTransparent("Sheld Transparent",Range(0,1)) = 0.7
		_EdgeColor("Edge Color",Color) = (1,1,1,1)
		_GridTex("Grid",2D) = "white"{}

	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		Pass
		{
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off
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
			float4 vertex : SV_POSITION;
			float4 screenPos : TEXCOORD1;
		};

		sampler2D _MainTex;
		sampler2D_float _CameraDepthTexture;
		sampler2D _GridTex;

		fixed4 _MainTint;
		fixed _MainTransparent;
		fixed4 _EdgeColor;

		v2f vert(appdata v)
		{
			v2f o;
			half3 worldSpaceVertex = mul(unity_ObjectToWorld, (v.vertex)).xyz;

			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			o.screenPos = ComputeScreenPos(o.vertex);
			return o;
		}

		half4 frag(v2f i) : SV_Target
		{
		half depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
		depth = LinearEyeDepth(depth);
		half diff = saturate(3*(depth - i.screenPos.w));

		half4 shieldEdge = tex2D(_MainTex, i.uv);
		shieldEdge.a = shieldEdge.r;
		
		half gridTransparent = tex2D(_GridTex, i.uv).a;
		gridTransparent *= sin(pow((i.uv.x-0.5),2)*20 + pow((i.uv.y - 0.5), 2)*20 - _Time.w)*0.5 + 0.5;

		_MainTint.a = gridTransparent + _MainTransparent;
		half4 intersectionOutline = lerp(_EdgeColor, fixed4(0,0,0,0),diff);

		
		
		shieldEdge += _MainTint + intersectionOutline;
		return shieldEdge;
	}
	ENDCG
}
	}
		Fallback "Transparent/Diffuse"
}
