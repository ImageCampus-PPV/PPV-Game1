Shader "Custom/PBRToonShaderFog"
{
    Properties
    {
        [Header(Base Texture)]
        [MainTexture] _BaseMap("Base Texture (RGB)", 2D) = "white" {}
        [MainColor] _BaseColor("Diffuse Color", Color) = (1,1,1,1)
        
        [Header(Normal)]
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0
        
        [Header(Emissive)]
        _EmissionMap("Emissive Map (RGB)", 2D) = "black" {}
        [HDR] _EmissionColor("Emissive Color", Color) = (0,0,0,1)
        _EmissionPower("Emissive Power", Range(1.0, 50.0)) = 1.0
        
        [Header(Height)]
        _ParallaxMap("Height Map (R)", 2D) = "black" {}
        _Parallax("Height Amplitude", Range(0.0, 0.1)) = 0.02
        
        [Header(Metallic AO Smoothness)]
        _MetallicGlossMap("Mask Map (Metallic: R, AO: G, Smoothness: A)", 2D) = "white" {}
        _Metallic("Metallic Intensity", Range(0.0, 1.0)) = 1.0
        _Smoothness("Smoothness Intensity", Range(0.0, 1.0)) = 1.0
        _OcclusionStrength("AO Strength", Range(0.0, 1.0)) = 1.0
        
        [Header(Toon Core Settings)]
        _LightingSteps("Toon Steps)", Range(1.0, 5.0)) = 2.0
        _LightingCutoff("Lighting Cutoff", Range(-1.0, 1.0)) = 0.0
        _FalloffAmount("Toon Edge Softness", Range(0.001, 0.5)) = 0.01
        _AmbientStrength("Ambient Shadow Density", Range(0.0, 1.0)) = 0.2
        
        [Header(Toon Stylized Details)]
        _FresnelColor("Toon Rim Color", Color) = (0,0,0,1)
        _FresnelSize("Toon Rim Thickness", Range(0.01, 1.0)) = 0.15
        _SpecularSize("Specular Sharpness", Range(0.01, 0.99)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD3;
                float2 uv           : TEXCOORD0;
                float3 normalWS     : TEXCOORD4;
                float3 tangentWS    : TEXCOORD5;
                float3 bitangentWS  : TEXCOORD6;
                float3 viewDirWS    : TEXCOORD7;
                float fogFactor     : TEXCOORD8; 
            };

            TEXTURE2D(_BaseMap);           SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);           SAMPLER(sampler_BumpMap);
            TEXTURE2D(_EmissionMap);       SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_ParallaxMap);       SAMPLER(sampler_ParallaxMap);
            TEXTURE2D(_MetallicGlossMap);  SAMPLER(sampler_MetallicGlossMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _BumpScale;
                half4 _EmissionColor;
                half _EmissionPower;
                half _Parallax;
                half _OcclusionStrength;
                half _Metallic;
                half _Smoothness;
                half _LightingSteps;
                half _LightingCutoff;
                half _FalloffAmount;
                half _AmbientStrength;
                half4 _FresnelColor;
                half _FresnelSize;
                half _SpecularSize;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDirWS = normalize(input.viewDirWS);
                
                float3 viewDirTS = float3(
                    dot(input.tangentWS, viewDirWS),
                    dot(input.bitangentWS, viewDirWS),
                    dot(input.normalWS, viewDirWS)
                );
                viewDirTS = normalize(viewDirTS);

                // 1. HEIGHT MAP (Parallax Mapping)
                float2 offset = viewDirTS.xy / (viewDirTS.z + 0.42) * (_Parallax * 0.1);
                float height = SAMPLE_TEXTURE2D(_ParallaxMap, sampler_ParallaxMap, input.uv).r;
                float2 uvDisplaced = input.uv + offset * height;

                // 2. TEXTURE SAMPLING
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvDisplaced) * _BaseColor;
                half3 normalSample = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvDisplaced), _BumpScale);
                
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uvDisplaced).rgb * _EmissionColor.rgb * _EmissionPower;

                half4 packedMask = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uvDisplaced);
                half customMetallic = packedMask.r * _Metallic;
                half ao = packedMask.g; 
                half customSmoothness = packedMask.a * _Smoothness;
                half aoStrength = lerp(1.0, ao, _OcclusionStrength);

                float3 normalWS = normalize(
                    normalSample.x * input.tangentWS + 
                    normalSample.y * input.bitangentWS + 
                    normalSample.z * input.normalWS
                );

                // 4. MAIN LIGHT (Multi-Banda Cel Shading)
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                
                half mainNdotL = dot(normalWS, mainLight.direction);
                
                half diffuseWeight = (mainNdotL * 0.5 + 0.5);
                half discreteDiffuse = floor(diffuseWeight * _LightingSteps) / _LightingSteps;
                half mainCelDiffuse = smoothstep(_LightingCutoff, _LightingCutoff + _FalloffAmount, discreteDiffuse);
                mainCelDiffuse = max(mainCelDiffuse, _AmbientStrength * aoStrength) * mainLight.shadowAttenuation;

                // Especular Toon
                float3 mainHalfWS = normalize(mainLight.direction + viewDirWS);
                half mainNdotH = max(0.0, dot(normalWS, mainHalfWS));
                half mainSpecular = pow(mainNdotH, lerp(4.0, 256.0, customSmoothness)) * customMetallic;
                half mainCelSpecular = smoothstep(_SpecularSize, _SpecularSize + _FalloffAmount, mainSpecular) * mainLight.shadowAttenuation;

                half3 finalColor = albedo.rgb * (mainLight.color * mainCelDiffuse) + (mainLight.color * mainCelSpecular);

                // 5. ADDITIONAL LIGHTS LOOP
                #if defined(_ADDITIONAL_LIGHTS)
                int pixelLightCount = GetAdditionalLightsCount();
                for (int i = 0; i < pixelLightCount; ++i)
                {
                    Light addLight = GetAdditionalLight(i, input.positionWS);
                    half addNdotL = dot(normalWS, addLight.direction);
                    
                    half addWeight = (addNdotL * 0.5 + 0.5);
                    half addDiscrete = floor(addWeight * _LightingSteps) / _LightingSteps;
                    half addCelDiffuse = smoothstep(_LightingCutoff, _LightingCutoff + _FalloffAmount, addDiscrete) * addLight.distanceAttenuation * addLight.shadowAttenuation;
                    
                    float3 addHalfWS = normalize(addLight.direction + viewDirWS);
                    half addNdotH = max(0.0, dot(normalWS, addHalfWS));
                    half addSpecular = pow(addNdotH, lerp(4.0, 256.0, customSmoothness)) * customMetallic;
                    half addCelSpecular = smoothstep(_SpecularSize, _SpecularSize + _FalloffAmount, addSpecular) * addLight.distanceAttenuation * addLight.shadowAttenuation;

                    finalColor += albedo.rgb * (addLight.color * addCelDiffuse) + (addLight.color * addCelSpecular);
                }
                #endif

                // 6. TOON SILHOUETTE / RIM LIGHT
                half fresnel = 1.0 - max(0.0, dot(normalWS, viewDirWS));
                half celFresnel = smoothstep(1.0 - _FresnelSize, 1.0 - _FresnelSize + _FalloffAmount, fresnel);
                
                half3 fresnelOutput = lerp(_FresnelColor.rgb, albedo.rgb * _FresnelColor.rgb, customMetallic);
                if(dot(_FresnelColor.rgb, float3(1,1,1)) < 0.1)
                {
                    finalColor = lerp(finalColor, _FresnelColor.rgb, celFresnel * 0.8);
                }
                else
                {
finalColor += fresnelOutput * celFresnel * (aoStrength + customMetallic);
}
// 7. EMISSION
finalColor += emission;
// 8. FOG INTERPOLATION
finalColor = MixFog(finalColor, input.fogFactor);
return half4(finalColor, albedo.a);
}
ENDHLSL
}
}
FallBack "UniversalRenderPipeline/SimpleLit"
}