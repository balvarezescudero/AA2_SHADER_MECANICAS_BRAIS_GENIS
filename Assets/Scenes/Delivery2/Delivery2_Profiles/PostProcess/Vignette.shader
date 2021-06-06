Shader "Hidden/Custom/Vignette"
{
	HLSLINCLUDE
		// StdLib.hlsl holds pre-configured vertex shaders (VertDefault), varying structs (VaryingsDefault), and most of the data you need to write common effects.
#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	float _intensity ;
	

	float random(float2 p)
	{
		return frac(sin(dot(p.xy, float2(_Time.y, 65.115))) * 2773.8856);
	}

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		   float4 mainColor = float4(100, 149, 230,1);
		   float2 texCoord = i.texcoord ;
		   

		   // Take texture color
		     float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

			 //Creamos el effecto viñeta

			 texCoord -= 0.5;

			 float vignette = 1.0 - dot(texCoord, texCoord);


			 //Determinamos La Cantidad de vignette que queremos
			   vignette = pow(vignette, _intensity); 
			   

			  color *= vignette;


		   return color ;
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
