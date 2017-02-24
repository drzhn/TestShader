Shader "Custom/ImageEffect" {
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_LuminosityAmount("GrayScale Amount", Range(0.0, 1)) = 1.0

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
			fixed _LuminosityAmount;

			fixed4 frag(v2f_img i) : COLOR
			{
				//Получим цвет из рендер-текстуры,
				//используя UV-координаты из структуры v2f_img.
				fixed4 renderTex = tex2D(_MainTex, i.uv);
			//Применим значение яркости к нашей рендер-текстуре.
			float luminosity = 0.299 * renderTex.r + 0.587 * renderTex.g + 0.114 * renderTex.b;
			fixed4 finalColor = lerp(renderTex, luminosity, _LuminosityAmount);
			//fixed4 finalColor = fixed4(1, 1, 1, 1) - renderTex;
			return finalColor;
		}
		ENDCG
		}
		}
			FallBack "Diffuse"
}
