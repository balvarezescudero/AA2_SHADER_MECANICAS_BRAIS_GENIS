Shader "Hidden/Custom/Pixelate"
{
	HLSLINCLUDE
		// StdLib.hlsl holds pre-configured vertex shaders (VertDefault), varying structs (VaryingsDefault), and most of the data you need to write common effects.
#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	float _intensity ;

	float4 Frag(VaryingsDefault i) : SV_Target
	{
	
		 float2 uv = i.texcoord;

		float pixelX = _ScreenParams.x/ _intensity;
		float pixelY = _ScreenParams.y/ _intensity;
		
		return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(floor(pixelX * uv.x) / pixelX, floor(pixelY * uv.y) / pixelY));
		
	}

	

		ENDHLSL

		SubShader
	{
		Cull Off ZWrite Off ZTest Always
			Pass
		{
			HLSLPROGRAM
				#pragma vertex VertDefault
				#pragma fragment Frag
			ENDHLSL
		}
	}
}
