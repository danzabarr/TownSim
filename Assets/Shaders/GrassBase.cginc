#ifndef GRASS_BASE_INCLUDE
#define GRASS_BASE_INCLUDE

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "Autolight.cginc"
#include "CustomTessellation.cginc"
#include "FogOfWar.cginc"
#include "PathMask.cginc"

uniform float _LatitudeScale;
uniform float _WaterLevel;
uniform float _AltitudeTemperature;
uniform float _Temperature;

struct geometryOutput
{
	float4 pos : SV_POSITION;
	float3 worldPos : TEXCOORD3;
	float2 uv : TEXCOORD0;
	float3 normal : NORMAL;

	float lighting : TEXCOORD5;

#if UNITY_PASS_SHADOWCASTER

#else
	unityShadowCoord4 _ShadowCoord : TEXCOORD1;
#endif
	float3 viewDir : TEXCOORD4;
	UNITY_FOG_COORDS(2)
};

// Simple noise function, sourced from http://answers.unity.com/answers/624136/view.html
// Extended discussion on this function can be found at the following link:
// https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
// Returns a number in the 0...1 range.
float rand(float3 co)
{
	return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
}

// Construct a rotation matrix that rotates around the provided axis, sourced from:
// https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
float3x3 AngleAxis3x3(float angle, float3 axis)
{
	float c, s;
	sincos(angle, s, c);

	float t = 1 - c;
	float x = axis.x;
	float y = axis.y;
	float z = axis.z;

	return float3x3(
		t * x * x + c, t * x * y - s * z, t * x * z + s * y,
		t * x * y + s * z, t * y * y + c, t * y * z - s * x,
		t * x * z - s * y, t * y * z + s * x, t * z * z + c
		);
}

geometryOutput VertexOutput(float3 pos, float3 normal, float2 uv, float lighting)
{
	geometryOutput o;
	o.pos = UnityObjectToClipPos(pos);
	o.uv = uv;
	o.normal = UnityObjectToWorldNormal(normal);
	o.lighting = lighting;

#if UNITY_PASS_SHADOWCASTER
	// Applying the bias prevents artifacts from appearing on the surface.
	o.pos = UnityApplyLinearShadowBias(o.pos);
#else
	o._ShadowCoord = ComputeScreenPos(o.pos);
#endif

	o.worldPos = mul(unity_ObjectToWorld, float4(pos.xyz, 1.0)).xyz;
	o.viewDir = normalize(ObjSpaceViewDir(float4(pos.xyz, 1.0))).xyz;
	UNITY_TRANSFER_FOG(o, o.pos);
#if UNITY_PASS_SHADOWCASTER
#else
	TRANSFER_SHADOW(o);
#endif
	
	return o;
}

geometryOutput GenerateGrassVertex(float3 vertexPosition, float width, float height, float forward, float2 uv, float3x3 transformMatrix, float lighting)
{
	float3 tangentPoint = float3(width, forward, height);

	float3 tangentNormal = normalize(float3(0, -1, forward));

	float3 localPosition = vertexPosition + mul(transformMatrix, tangentPoint);
	float3 localNormal = mul(transformMatrix, tangentNormal);
		 
	return VertexOutput(localPosition, localNormal, uv, lighting);
}


float _LocalUp;

float _BladeHeight;
float _BladeHeightRandom;

float _BladeWidthRandom;
float _BladeWidth;

float _BladeForward;
float _BladeCurve;

float _BendRotationRandom;

float _MinSize;

sampler2D _WindDistortionMap;
float4 _WindDistortionMap_ST;

float _WindStrength;
float2 _WindFrequency;

float _AltMin;
float _AltMax;
float _AltMinBlend;
float _AltMaxBlend;

float _SlopeMax;
float _SlopeBlend;

sampler2D _MaskTex;
float4 _MaskTex_ST;
float _MaskTexThreshold;
float _MaskTexBlend;

#define BLADE_SEGMENTS 4

// Geometry program that takes in a single triangle and outputs a blade
// of grass at that triangle first vertex position, aligned to the vertex's normal.
[maxvertexcount(BLADE_SEGMENTS * 2 + 1)]
void geo(triangle vertexOutput IN[3], inout TriangleStream<geometryOutput> triStream)
{
	float3 pos = (IN[0].vertex.xyz + IN[1].vertex.xyz + IN[2].vertex.xyz) / 3;
	float3 world = mul(unity_ObjectToWorld, float4(pos.xyz, 1.0)).xyz;
	float3 normal = (IN[0].normal.xyz + IN[1].normal.xyz + IN[2].normal.xyz) / 3;

	float3 lightDir = normalize(UnityWorldSpaceLightDir(world));

	float lighting = saturate(dot(lightDir, IN[0].normal));
	
	float3 viewDir = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));

	float bladeSize = 1;
	float bladeWidth = 1;
	float bladeHeight = 1;

	bladeSize *= saturate((world.y - _AltMin) / _AltMinBlend);
	bladeSize *= saturate((_AltMax - world.y) / _AltMaxBlend);

	bladeSize *= saturate((normal.y - _SlopeMax) / _SlopeBlend);

	float3 maskTextureSample = tex2Dlod(_MaskTex, float4(TRANSFORM_TEX(world.xz, _MaskTex), 0, 0)).rgb;
	float maskTextureValue = (maskTextureSample.x + maskTextureSample.y + maskTextureSample.z) / 3;
	float pathTextureValue = SamplePathMask(world.xz);

	maskTextureValue = min(maskTextureValue, 1 - pathTextureValue);

	bladeSize *= saturate((maskTextureValue - _MaskTexThreshold) / _MaskTexBlend);
	
	if (bladeSize < _MinSize)
		return;

	// Each blade of grass is constructed in tangent space with respect
	// to the emitting vertex's normal and tangent vectors, where the width
	// lies along the X axis and the height along Z.

	// Construct random rotations to point the blade in a direction.
	float3x3 facingRotationMatrix = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1));
	// Matrix to bend the blade in the direction it's facing.
	float3x3 bendRotationMatrix = AngleAxis3x3(rand(pos.zzx) * _BendRotationRandom * UNITY_PI * 0.5, float3(-1, 0, 0));

	// Sample the wind distortion map, and construct a normalized vector of its direction.
	float2 uv = world.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
	float2 windSample = (tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy * 2 - 1) * _WindStrength;
	float3 wind = normalize(float3(windSample.x, windSample.y, 0));

	float3x3 windRotation = AngleAxis3x3(UNITY_PI * windSample, wind);

	// Construct a matrix to transform our blade from tangent space
	// to local space; this is the same process used when sampling normal maps.
	float3 vNormal = lerp(IN[0].normal, float3(0, 1, 0), _LocalUp);
	float4 vTangent = IN[0].tangent;
	float3 vBinormal = cross(vNormal, vTangent) * vTangent.w;

	float3x3 tangentToLocal = float3x3(
		vTangent.x, vBinormal.x, vNormal.x,
		vTangent.y, vBinormal.y, vNormal.y,
		vTangent.z, vBinormal.z, vNormal.z
	);

	// Construct full tangent to local matrix, including our rotations.
	// Construct a second matrix with only the facing rotation; this will be used 
	// for the root of the blade, to ensure it always faces the correct direction.
	float3x3 transformationMatrix = mul(mul(mul(tangentToLocal, windRotation), facingRotationMatrix), bendRotationMatrix);
	float3x3 transformationMatrixFacing = mul(tangentToLocal, facingRotationMatrix);

	float height = ((rand(pos.zyx) * 2 - 1) * _BladeHeightRandom + _BladeHeight) * bladeHeight * bladeSize;
	float width = ((rand(pos.xzy) * 2 - 1) * _BladeWidthRandom + _BladeWidth) * bladeWidth * bladeSize;
	float forward = rand(pos.yyz) * _BladeForward;


	for (int i = 0; i < BLADE_SEGMENTS; i++)
	{
		float t = i / (float)BLADE_SEGMENTS;

		float segmentHeight = height * t;
		float segmentWidth = width * (1 - t);
		float segmentForward = pow(t, _BladeCurve) * forward;

		// Select the facing-only transformation matrix for the root of the blade.
		float3x3 transformMatrix = i == 0 ? transformationMatrixFacing : transformationMatrix;

		triStream.Append(GenerateGrassVertex(pos, segmentWidth, segmentHeight, segmentForward, float2(0, t), transformMatrix, lighting));
		triStream.Append(GenerateGrassVertex(pos, -segmentWidth, segmentHeight, segmentForward, float2(1, t), transformMatrix, lighting));
	}

	// Add the final vertex as the tip.
	triStream.Append(GenerateGrassVertex(pos, 0, height, forward, float2(0.5, 1), transformationMatrix, lighting));
}

#endif