﻿Shader "Custom/CustomDotsInstancingShader"
{
	Properties
	{
		_BaseColor("Base Color", Color) = (1,1,1,1)
		_MainTex("Base Map", 2D) = "white" {}
	}
		SubShader
		{
			Tags
			{
				"RenderPipeline"="UniversalPipeline"
				"RenderType" = "Opaque"
				"Queue" = "Geometry"
			}
			Pass
			{
				Name "Pass"

				HLSLPROGRAM

				#pragma target 4.5
				#pragma multi_compile_instancing
				#pragma multi_compile _ DOTS_INSTANCING_ON
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
				#pragma multi_compile _ _ADDITIONAL_LIGHTS

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

				struct Attributes
				{
					float4 positionOS : POSITION;
					float2 uv : TEXCOORD0;
					float3 normalOS : NORMAL;

					#if UNITY_ANY_INSTANCING_ENABLED
					uint instanceID : INSTANCEID_SEMANTIC;
					#endif
				};

				struct Varyings
				{
					float4 positionHCS : SV_POSITION;
					float2 uv : TEXCOORD0;
					float3 normalWS : TEXCOORD1;
					float3 positionWS : TEXCOORD2;
					float3 color : TEXCOORD3;

					#if UNITY_ANY_INSTANCING_ENABLED
					uint instanceID : CUSTOM_INSTANCE_ID;
					#endif
				};

				CBUFFER_START(UnityPerMaterial)
					float4 _BaseColor;
				CBUFFER_END

				#if defined(UNITY_DOTS_INSTANCING_ENABLED)
					UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
						UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
						UNITY_DOTS_INSTANCED_PROP(float4, _MainTex)
					UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

					#define _Color UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _Color)
					#define _MainTex_ST UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _MainTex)
				#endif

				TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

				Varyings vert(Attributes input)
				{
					Varyings output;

					UNITY_SETUP_INSTANCE_ID(input);
					UNITY_TRANSFER_INSTANCE_ID(input, output);

					float3 posWS = TransformObjectToWorld(input.positionOS);
					output.positionHCS = TransformWorldToHClip(posWS.xyz);
					output.positionWS = posWS.xyz;
					output.normalWS = TransformObjectToWorldNormal(input.normalOS);
					output.uv = input.uv;

					float3 worldPosition = output.positionWS;

					float dist = length(worldPosition.xz);

					float minDist = 20.0 - 10.0;
					float maxDist = 100.0 + 30.0;
					float t = saturate((dist - minDist) / (maxDist - minDist));

					float3 k = float3(0.0, 2.094f, 4.188f);
					float3 rainbow = 0.5 + 0.5 * cos(k - t * 6.28318);

					float3 heightCol = float3(worldPosition.y, worldPosition.y, worldPosition.y);

					output.color = float4(rainbow + (heightCol * 0.025f), 1.0);


					#if UNITY_ANY_INSTANCING_ENABLED
					output.instanceID = input.instanceID;
					#endif

					return output;
				}

				half4 frag(Varyings input) : SV_TARGET
				{
					float3 normal = normalize(input.normalWS);
					float3 lightDir = normalize(_MainLightPosition.xyz);
					float3 lightColor = _MainLightColor.rgb;

					float NdotL = saturate(dot(normal, lightDir));
					float4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
					float3 albedo = baseMap.rgb * input.color.rgb;

					float3 color = albedo * lightColor * NdotL;

					return float4(color, 1.0);
				}

				ENDHLSL
			}
		}
}