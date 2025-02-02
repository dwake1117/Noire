#ifndef FLATKIT_LIGHTING_DR_INCLUDED
#define FLATKIT_LIGHTING_DR_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

inline half NdotLTransition(half3 normal, half3 lightDir, half selfShadingSize, half shadowEdgeSize, half flatness) {
    const half NdotL = dot(normal, lightDir);
    const half angleDiff = saturate((NdotL * 0.5 + 0.5) - selfShadingSize);
    const half angleDiffTransition = smoothstep(0, shadowEdgeSize, angleDiff); 
    return lerp(angleDiff, angleDiffTransition, flatness);
}

inline half NdotLTransitionPrimary(half3 normal, half3 lightDir) { 
    return NdotLTransition(normal, lightDir, _SelfShadingSize, _ShadowEdgeSize, _Flatness);
}

#if defined(DR_CEL_EXTRA_ON)
inline half NdotLTransitionExtra(half3 normal, half3 lightDir) { 
    return NdotLTransition(normal, lightDir, _SelfShadingSizeExtra, _ShadowEdgeSizeExtra, _FlatnessExtra);
}
#endif

inline half NdotLTransitionTexture(half3 normal, half3 lightDir, sampler2D stepTex) {
    const half NdotL = dot(normal, lightDir);
    const half angleDiff = saturate((NdotL * 0.5 + 0.5) - _SelfShadingSize * 0.0);
    const half4 rampColor = tex2D(stepTex, half2(angleDiff, 0.5));
    // NOTE: The color channel here corresponds to the texture format in the shader editor script.
    const half angleDiffTransition = rampColor.r;
    return angleDiffTransition;
}

half3 LightingPhysicallyBased_DSTRM(Light light, InputData inputData)
{
    // If all light in the scene is baked, we use custom light direction for the cel shading.
    light.direction = lerp(light.direction, _LightmapDirection, _OverrideLightmapDir);

    half4 c = _BaseColor;

#if defined(_CELPRIMARYMODE_SINGLE)
    const half NdotLTPrimary = NdotLTransitionPrimary(inputData.normalWS, light.direction);
    c = lerp(_ColorDim, c, NdotLTPrimary);
#endif  // _CELPRIMARYMODE_SINGLE

#if defined(_CELPRIMARYMODE_STEPS)
    const half NdotLTSteps = NdotLTransitionTexture(inputData.normalWS, light.direction, _CelStepTexture);
    c = lerp(_ColorDimSteps, c, NdotLTSteps);
#endif  // _CELPRIMARYMODE_STEPS

#if defined(_CELPRIMARYMODE_CURVE)
    const half NdotLTCurve = NdotLTransitionTexture(inputData.normalWS, light.direction, _CelCurveTexture);
    c = lerp(_ColorDimCurve, c, NdotLTCurve);
#endif  // _CELPRIMARYMODE_CURVE

#if defined(DR_CEL_EXTRA_ON)
    const half NdotLTExtra = NdotLTransitionExtra(inputData.normalWS, light.direction);
    c = lerp(_ColorDimExtra, c, NdotLTExtra);
#endif  // DR_CEL_EXTRA_ON

#if defined(DR_GRADIENT_ON)
    const float angleRadians = _GradientAngle / 180.0 * PI;
    const float posGradRotated = (inputData.positionWS.x - _GradientCenterX) * sin(angleRadians) + 
                           (inputData.positionWS.y - _GradientCenterY) * cos(angleRadians);
    const float gradientTop = _GradientCenterY + _GradientSize * 0.5;
    const half gradientFactor = saturate((gradientTop - posGradRotated) / _GradientSize);
    c = lerp(c, _ColorGradient, gradientFactor);
#endif  // DR_GRADIENT_ON

    const half NdotL = dot(inputData.normalWS, light.direction);

#if defined(DR_RIM_ON)
    const float rim = 1.0 - dot(inputData.viewDirectionWS, inputData.normalWS);
    const float rimSpread = 1.0 - _FlatRimSize - NdotL * _FlatRimLightAlign;
    const float rimEdgeSmooth = _FlatRimEdgeSmoothness;
    const float rimTransition = smoothstep(rimSpread - rimEdgeSmooth * 0.5, rimSpread + rimEdgeSmooth * 0.5, rim);
    c.rgb = lerp(c.rgb, _FlatRimColor.rgb, rimTransition * _FlatRimColor.a);
#endif  // DR_RIM_ON

#if defined(DR_SPECULAR_ON)
    // Halfway between lighting direction and view vector.
    
    // const float3 halfVector = normalize(light.direction + inputData.viewDirectionWS);
    // RIGHT NOW WE ARE NOT FIXING CAMERA ANGLE, SO WE WILL NOT USE inputData.viewDirectionWS
    const float3 halfVector = normalize(light.direction);
    
    const float NdotH = dot(inputData.normalWS, halfVector) * 0.5 + 0.5;
    const float specular = saturate(pow(abs(NdotH), 100.0 * (1.0 - _FlatSpecularSize) * (1.0 - _FlatSpecularSize)));
    const float specularTransition = smoothstep(0.5 - _FlatSpecularEdgeSmoothness * 0.1,
                                                0.5 + _FlatSpecularEdgeSmoothness * 0.1, specular);
    c = lerp(c, _FlatSpecularColor, specularTransition);
#endif  // DR_SPECULAR_ON

#if defined(_UNITYSHADOW_OCCLUSION)
    const float occludedAttenuation = smoothstep(0.25, 0.0, -min(NdotL, 0));
    light.shadowAttenuation *= occludedAttenuation;
    light.distanceAttenuation *= occludedAttenuation;
#endif

#if defined(_UNITYSHADOWMODE_MULTIPLY)
    c *= lerp(1, light.shadowAttenuation, _UnityShadowPower);
#endif
#if defined(_UNITYSHADOWMODE_COLOR)
    c = lerp(lerp(c, _UnityShadowColor, _UnityShadowColor.a), c, light.shadowAttenuation);
#endif

    c.rgb *= light.color * light.distanceAttenuation;

    return c.rgb;
}

void StylizeLight(inout Light light)
{
    const half shadowAttenuation = saturate(light.shadowAttenuation * _UnityShadowSharpness);
    light.shadowAttenuation = shadowAttenuation;

    const float distanceAttenuation = smoothstep(0, _LightFalloffSize + 0.001, light.distanceAttenuation);
    light.distanceAttenuation = distanceAttenuation;

    const half3 lightColor = lerp(half3(1, 1, 1), light.color, _LightContribution);
    light.color = lightColor;
}

half4 UniversalFragment_DSTRM(InputData inputData, SurfaceData surfaceData, float2 uv)
{
    // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    const half4 shadowMask = inputData.shadowMask;
    #elif !defined (LIGHTMAP_ON)
    const half4 shadowMask = unity_ProbesOcclusion;
    #else
    const half4 shadowMask = half4(1, 1, 1, 1);
    #endif

    #if VERSION_GREATER_EQUAL(10, 0)
    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    #else
    Light mainLight = GetMainLight(inputData.shadowCoord);
    #endif

	#if UNITY_VERSION >= 202220
    uint meshRenderingLayers = GetMeshRenderingLayer();
    #elif VERSION_GREATER_EQUAL(12, 0)
    uint meshRenderingLayers = GetMeshRenderingLightLayer();
    #endif

#if LIGHTMAP_ON
    mainLight.distanceAttenuation = 1.0;
#endif
    StylizeLight(mainLight);

    #if defined(_SCREEN_SPACE_OCCLUSION)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
        mainLight.color *= aoFactor.directAmbientOcclusion;
        inputData.bakedGI *= aoFactor.indirectAmbientOcclusion;
    #endif

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, shadowMask);

    // Apply Flat Kit stylizing to `inputData.bakedGI` (which is half3).
#if LIGHTMAP_ON
    #if defined(_UNITYSHADOWMODE_MULTIPLY)
        inputData.bakedGI *= _UnityShadowPower;
    #endif
    #if defined(_UNITYSHADOWMODE_COLOR)
        float giLength = length(inputData.bakedGI);
        inputData.bakedGI = lerp(giLength, _UnityShadowColor.rgb, _UnityShadowColor.a * giLength);
    #endif
#endif

    const half4 albedo = half4(surfaceData.albedo + surfaceData.emission, surfaceData.alpha);
    const half4 detail = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, uv);

    #if defined(_BASEMAP_PREMULTIPLY)
        half3 brdf = albedo.rgb;
    #else
        half3 brdf = _BaseColor.rgb;
    #endif
    
    BRDFData brdfData;
    InitializeBRDFData(brdf, 1.0 - 1.0 / kDielectricSpec.a, 0, 0, surfaceData.alpha, brdfData);
    half3 color = GlobalIllumination(brdfData, inputData.bakedGI, 1.0, inputData.normalWS, inputData.viewDirectionWS);
    #if VERSION_GREATER_EQUAL(12, 0)
	#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
	#endif
    #endif
    color += LightingPhysicallyBased_DSTRM(mainLight, inputData);

#ifdef _ADDITIONAL_LIGHTS
    const uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        #if VERSION_GREATER_EQUAL(10, 0)
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        #else
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
        #endif

        #if defined(_SCREEN_SPACE_OCCLUSION)
            light.color *= aoFactor.directAmbientOcclusion;
        #endif

        StylizeLight(light);
        #if VERSION_GREATER_EQUAL(12, 0)
		#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		#endif
        #endif
        color += LightingPhysicallyBased_DSTRM(light, inputData);
    }
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    color += inputData.vertexLighting * brdfData.diffuse;
#endif

    // Base map.
    {
        #if defined(_TEXTUREBLENDINGMODE_ADD)
        color += lerp(half3(0.0f, 0.0f, 0.0f), albedo.rgb, _TextureImpact);
        #else  // _TEXTUREBLENDINGMODE_MULTIPLY
        color *= lerp(half3(1.0f, 1.0f, 1.0f), albedo.rgb, _TextureImpact);
        #endif
    }

    // Detail map.
    {
        #if defined(_DETAILMAPBLENDINGMODE_ADD)
        color += lerp(0, _DetailMapColor.rgb, detail.rgb * _DetailMapImpact).rgb;
        #endif
        #if defined(_DETAILMAPBLENDINGMODE_MULTIPLY)
        color *= lerp(1, _DetailMapColor.rgb, detail.rgb * _DetailMapImpact).rgb;
        #endif
        #if defined(_DETAILMAPBLENDINGMODE_INTERPOLATE)
        color = lerp(color, detail.rgb, _DetailMapImpact * _DetailMapColor.rgb * detail.a).rgb;
        #endif
    }

    color += surfaceData.emission;
    return half4(color, surfaceData.alpha);
}

#endif // FLATKIT_LIGHTING_DR_INCLUDED