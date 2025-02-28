Shader "Custom/URPWaterWithDepth"
{
    Properties
    {
        [Header(Base Settings)]
        _WaterColor("浅水颜色", Color) = (0.2, 0.6, 1, 0.8)
        _DeepWaterColor("深水颜色", Color) = (0.1, 0.3, 0.5, 1)
        _NormalMap("法线贴图", 2D) = "bump" {}
        _NormalScale("法线强度", Range(0,2)) = 1

        [Header(Refraction)]
        _RefractionStrength("折射强度", Range(0, 0.1)) = 0.05
        _RefractionDistort("折射扭曲", Range(0,2)) = 1

        [Header(Reflection)]
        _ReflectionStrength("反射强度", Range(0,1)) = 0.5
        _FresnelPower("菲涅尔强度", Range(0,5)) = 2

        [Header(Depth)]
        _DepthMaxDistance("深度影响距离", Float) = 5
        _DepthFade("深度过渡", Range(0.1,5)) = 1
        _DepthAlpha("透明度衰减", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent+0"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float3 tangentWS : TEXCOORD5;
                float3 bitangentWS : TEXCOORD6;
            };

            // Texture Samplers
            TEXTURE2D(_NormalMap);        SAMPLER(sampler_NormalMap);
            TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);
            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _WaterColor;
                float4 _DeepWaterColor;
                float4 _NormalMap_ST;
                float _NormalScale;
                float _RefractionStrength;
                float _RefractionDistort;
                float _ReflectionStrength;
                float _FresnelPower;
                float _DepthMaxDistance;
                float _DepthFade;
                float _DepthAlpha;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _NormalMap);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.viewDirWS = GetWorldSpaceNormalizeViewDir(o.positionWS);
                o.screenPos = ComputeScreenPos(o.positionCS);

                // 计算TBN矩阵
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                o.normalWS = normalInput.normalWS;
                o.tangentWS = normalInput.tangentWS;
                o.bitangentWS = normalInput.bitangentWS;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // ================== 法线计算 ==================
                float2 uv = i.uv + float2(_Time.y * 0.1, 0);
                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv), _NormalScale);
                normalTS.xy *= _RefractionDistort;

                float3x3 TBN = float3x3(
                    normalize(i.tangentWS),
                    normalize(i.bitangentWS),
                    normalize(i.normalWS)
                );
                float3 normalWS = normalize(mul(normalTS, TBN));

                // ================== 深度计算 ==================
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV).r;
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float waterSurfaceDepth = i.positionWS.y - _WorldSpaceCameraPos.y;
                float waterDepth = sceneDepth - waterSurfaceDepth;
                
                float depthFactor = saturate(waterDepth / _DepthMaxDistance);
                depthFactor = pow(depthFactor, _DepthFade);

                // ================== 折射计算 ==================
                float2 refractionOffset = normalWS.xy * _RefractionStrength * (1 - depthFactor);
                half3 refraction = SAMPLE_TEXTURE2D_LOD(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + refractionOffset, 0).rgb;

                // ================== 反射计算 ==================
                float3 reflectionVector = reflect(-i.viewDirWS, normalWS);
                float3 reflection = GlossyEnvironmentReflection(reflectionVector, 0.02, 1.0);
                float fresnel = pow(saturate(1.0 - dot(normalWS, i.viewDirWS)), _FresnelPower);
                half3 reflectionColor = reflection * _ReflectionStrength * fresnel;

                // ================== 颜色混合 ==================
                half3 waterColor = lerp(_WaterColor.rgb, _DeepWaterColor.rgb, depthFactor);
                half3 finalColor = lerp(refraction, reflectionColor, fresnel);
                finalColor = lerp(finalColor, waterColor, depthFactor * 0.5);

                // ================== 透明度 ==================
                float alpha = lerp(_WaterColor.a, _DeepWaterColor.a, depthFactor);
                alpha *= lerp(1.0, _DepthAlpha, depthFactor);

                // ================== 光照增强 ==================
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                finalColor += mainLight.color * NdotL * 0.2;

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Simple Lit"
}