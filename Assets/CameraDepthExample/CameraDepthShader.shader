﻿Shader "Custom/CameraDepthShader" {
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_DepthPower("Depth Power", Range(1, 5)) = 1
	}
	SubShader 
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			uniform sampler2D _MainTex;
			fixed _DepthPower;
			sampler2D _CameraDepthTexture;

			fixed4 frag(v2f_img i) : COLOR
			{
				//Получим цвет из рендер-текстуры,
				//используя UV-координаты из структуры v2f_img.
				float d = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,float2(i.uv.x,1 - i.uv.y)));
				d = pow(Linear01Depth(d), _DepthPower);
				return d;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
