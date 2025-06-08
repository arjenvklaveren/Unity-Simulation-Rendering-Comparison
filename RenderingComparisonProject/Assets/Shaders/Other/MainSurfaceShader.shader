Shader "Custom/MainSurfaceShader"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _MainTex("Base Map", 2D) = "white" {}
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Particle
            {
                float4 position;
            };
            StructuredBuffer<Particle> particleBuffer;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float3 color : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            Varyings vert(Attributes input)
            {
                Varyings output;
                float4 bufferPosition = particleBuffer[input.instanceID].position;
                float3 localPosition = input.positionOS.xyz * bufferPosition.w;
                float3 worldPosition = bufferPosition + localPosition;
                float3 posWS = TransformObjectToWorld(input.positionOS);

                output.positionHCS = TransformWorldToHClip(worldPosition.xyz);
                output.positionWS = posWS.xyz;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;

                float dist = length(worldPosition.xz);

                float minDist = 20.0 - 10.0;
                float maxDist = 100.0 + 30.0;
                float t = saturate((dist - minDist) / (maxDist - minDist));

                float3 k = float3(0.0, 2.094f, 4.188f);
                float3 rainbow = 0.5 + 0.5 * cos(k - t * 6.28318);

                float3 heightCol = float3(bufferPosition.y, bufferPosition.y, bufferPosition.y);

                output.color = float4(rainbow + (heightCol * 0.025f), 1.0);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
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