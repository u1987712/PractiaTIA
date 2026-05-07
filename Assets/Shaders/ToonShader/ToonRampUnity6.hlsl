void ToonShading_float(
    in float3 Normal,
    in float ToonRampSmoothness,
    in float4 ClipSpacePos,
    in float3 WorldPos,
    in float3 ToonRampTinting,
    in float ToonRampOffset,
    in float ToonRampOffsetPoint,
    in float Ambient,
    out float3 ToonRampOutput
)
{

	// vista previa del nodo en Shader Graph
	#ifdef SHADERGRAPH_PREVIEW
		ToonRampOutput = float3(0.5,0.5,0);
	#else

		// obtener coordenadas de sombras
		#if SHADOWS_SCREEN
			half4 shadowCoord = ComputeScreenPos(ClipSpacePos);
		#else
			half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
		#endif 

		// obtener la luz principal (Directional Light)
		#if _MAIN_LIGHT_SHADOWS_CASCADE || _MAIN_LIGHT_SHADOWS
			Light light = GetMainLight(shadowCoord);
		#else
			Light light = GetMainLight();
		#endif

		// producto escalar para iluminación base toon
		half d = dot(Normal, light.direction) * 0.5 + 0.5;
		
		// ramp toon con suavizado
		half toonRamp = smoothstep(ToonRampOffset, ToonRampOffset + ToonRampSmoothness, d );
		
		float3 extraLights = float3(0,0,0);

		// InputData (solo si lo necesitas para otros efectos futuros)
   		InputData inputData = (InputData)0;
        inputData.positionWS = WorldPos;
        inputData.normalWS = Normal;
        inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(WorldPos);

		// luces adicionales (point/spot)
		uint lightsCount = GetAdditionalLightsCount();
        LIGHT_LOOP_BEGIN(lightsCount)
            
            Light aLight = GetAdditionalLight(lightIndex, WorldPos, half4(1,1,1,1));

			// NdotL luces adicionales
			half d = dot(Normal, aLight.direction) * 0.5 + 0.5;

			// atenuación de luz
			float3 attenuatedLightColor = aLight.color * (aLight.distanceAttenuation * aLight.shadowAttenuation);

			// toon ramp para luces adicionales
			half toonRampExtra = smoothstep(
				ToonRampOffsetPoint,
				ToonRampOffsetPoint + ToonRampSmoothness,
				d
			);

			// acumulación
			extraLights += (toonRampExtra * attenuatedLightColor);
					
        LIGHT_LOOP_END
		
		// sombras en luz principal
		toonRamp *= light.shadowAttenuation;

		// salida final
		ToonRampOutput = light.color * (toonRamp + ToonRampTinting) + Ambient;

		// añadir luces puntuales / spot
		ToonRampOutput += extraLights;

	#endif
}