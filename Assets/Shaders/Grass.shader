// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Roystan/Grass"
{
    Properties
    {
		_TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1
		[Space]
		[Header(Shading)]
		[Space]
		_TopColor("Top Color", Color) = (1,1,1,1)
		_BottomColor("Bottom Color", Color) = (1,1,1,1)
		_Specular("Specular Lighting", Range(0, 2)) = 1
		_ModelDiffuse("Model Diffuse Light", Range(0, 1)) = 1
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5
		_Smoothness("Smoothness", Range(0, 1)) = .5
		_Ambient("Ambient Light", Float) = 1

		[Header(Blades)]
		[Space]
		_LocalUp("Local Up", Range(0, 1)) = 1
		_BladeWidth("Blade Width", Float) = 0.05
		_BladeWidthRandom("Blade Width Random", Float) = 0.02
		_BladeHeight("Blade Height", Float) = 0.5
		_BladeHeightRandom("Blade Height Random", Float) = 0.3
		_BladeForward("Blade Forward Amount", Float) = 0.38
		_BladeCurve("Blade Curvature Amount", Range(1, 4)) = 2
		_BendRotationRandom("Bend Rotation Random", Range(0, 1)) = 0.2
		_MinSize("Minimum Size", Range(0, 1)) = 0

		[Header(Wind)]
		[Space]
		_WindDistortionMap("Wind Distortion Map", 2D) = "clear" {}
		_WindStrength("Wind Strength", Float) = 1
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)

		[Header(Minimum Altitude)]
		[Space]
		_AltMin("Minimum Altitude", Float) = 0
		_AltMinBlend("Min Alt Blending", Float) = 0
		
		[Header(Maximum Altitude)]
		[Space]
		_AltMax("Maximum Altitude", Float) = 0
		_AltMaxBlend("Max Alt Blending", Float) = 0
		
		[Header(Slope)]
		[Space]
		_SlopeMax("Maximum Slope", Range(-1, 1)) = 0
		_SlopeBlend("Slope Blending", Float) = 0

		[Header(Texture Masking)]
		[Space]
		_MaskTex("Mask Texture", 2D) = "white" {}
		_MaskTexThreshold("Mask Threshold", Range(0, 1)) = 0
		_MaskTexBlend("Mask Blending", Float) = 0
	}

    SubShader
    {
		Tags
		{
			"Queue" = "Transparent"

		}
		Cull Off
        Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
				"LightMode" = "ForwardBase"
			}

            CGPROGRAM
            #pragma vertex vert
			#pragma geometry geo
            #pragma fragment frag
			#pragma hull hull
			#pragma domain domain
			#pragma target 4.6
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog

			#pragma shader_feature FOG_OF_WAR
			#pragma shader_feature APPLY_TEMPERATURE
			#pragma shader_feature APPLY_CAMERA_MASK
			#pragma shader_feature APPLY_ALTITUDE_CULLING

			#include "GrassBase.cginc"

			float3 _TopColor;
			float3 _BottomColor;
			float _TranslucentGain;
			float _Ambient;
			float _Specular;
			float _Smoothness;
			float _ModelDiffuse;
			float _BladeDiffuse;


			float4 frag (geometryOutput i, fixed facing : VFACE) : SV_Target
            {			
				half3 col = lerp(_BottomColor, _TopColor, i.uv.y);
				half3 normal = facing > 0 ? i.normal : -i.normal;
				
				half diffuse = saturate(saturate(dot(normal, _WorldSpaceLightPos0)) + _TranslucentGain);
				half modelDiffuse = i.lighting * _ModelDiffuse;
				
				half3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos.xyz));
				half specular = pow(max(0.0, dot(reflect(-lightDir, normal), i.viewDir)), _Smoothness) * _Specular;
				
				float atten = UNITY_SHADOW_ATTENUATION(i, i.worldPos);

				col *= max((diffuse + specular + modelDiffuse) * atten  * _LightColor0, UNITY_LIGHTMODEL_AMBIENT * _Ambient);
				col = saturate(col);

				UNITY_APPLY_FOG(i.fogCoord, col);

		#ifdef FOG_OF_WAR
				ApplyFogOfWar(i.worldPos, col);
		#endif

				return float4(col, 1);
			}

            ENDCG
        }

		Pass
		{
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			#pragma hull hull
			#pragma domain domain
			#pragma target 4.6
			#pragma multi_compile_shadowcaster
			#include "GrassBase.cginc"

			float4 frag(geometryOutput i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
		}

		Pass
		{
			Tags {
				"LightMode" = "ForwardAdd"
				"Queue" = "Transparent"
			}

			Blend One One
			ZWrite Off
			CGPROGRAM

			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			#pragma hull hull
			#pragma domain domain
			#pragma target 4.6

			#define POINT

			#include "AutoLight.cginc"
			#include "GrassBase.cginc"
			#include "UnityPBSLighting.cginc"

			float _Smoothness;
			float _TranslucentGain;

			UnityLight CreateLight (float3 worldPos, float3 normal) {
				UnityLight light;
				light.dir = normalize(_WorldSpaceLightPos0.xyz - worldPos);

				float d = length(_WorldSpaceLightPos0.xyz - worldPos);
				float range = 10;
				float normalizedDist = d / range;
				float attenuation = saturate(1.0 / (1.0 + 25.0 * normalizedDist * normalizedDist) * saturate((1 - normalizedDist) * 5.0));

				light.color = _LightColor0.rgb * attenuation;
				return light;
			}

			float4 frag (geometryOutput i, fixed facing : VFACE) : SV_Target
			{
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				float3 albedo = 0;
				float metallic = 0;
				half3 normal = facing > 0 ? i.normal : -i.normal;
				normal = lerp(normal, normalize(_WorldSpaceLightPos0.xyz - i.worldPos), _TranslucentGain);
				float smoothness = _Smoothness;
				float3 specularTint;
				float oneMinusReflectivity;

				albedo = DiffuseAndSpecularFromMetallic
				(
					albedo, metallic, specularTint, oneMinusReflectivity
				);

				UnityIndirect indirectLight;
				indirectLight.diffuse = 0;
				indirectLight.specular = 0;

				float4 result = UNITY_BRDF_PBS(
					albedo, specularTint,
					oneMinusReflectivity, smoothness,
					normal, viewDir,
					CreateLight(i.worldPos, normal), indirectLight
				);

				result = clamp(result, 0, 1);

				

				return result;
			}

			ENDCG
		}
    }
}
