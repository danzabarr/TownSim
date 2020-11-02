#ifndef PATH_MASK_INCLUDE
#define PATH_MASK_INCLUDE

sampler2D _PathMaskTex;
float4 _PathMaskSize;

float SamplePathMask(float2 worldXZ)
{
	float x = (worldXZ.x - _PathMaskSize.x + _PathMaskSize.z) / (_PathMaskSize.z * 2);
	float y = (worldXZ.y - _PathMaskSize.y + _PathMaskSize.w) / (_PathMaskSize.w * 2);

	fixed4 maskSample = tex2Dlod(_PathMaskTex, float4(x, y, 0, 0));

	return (maskSample.r + maskSample.g + maskSample.b) / 3;
}

#endif