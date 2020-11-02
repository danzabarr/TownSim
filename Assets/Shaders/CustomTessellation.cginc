// Tessellation programs based on this article by Catlike Coding:
// https://catlikecoding.com/unity/tutorials/advanced-rendering/tessellation/

struct vertexInput
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD;
};

struct vertexOutput
{
	float4 vertex : SV_POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD;
};

struct TessellationFactors 
{
	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

vertexInput vert(vertexInput v)
{
	return v;
}

sampler2D _GrassMask;
float4 _GrassMask_ST;
float _TessellationUniform;
float4 _BillboardSize;
float _DistanceCulling;

vertexOutput tessVert(vertexInput v)
{
	vertexOutput o;
	// Note that the vertex is NOT transformed to clip
	// space here; this is done in the grass geometry shader.
	o.vertex = v.vertex;
	o.normal = v.normal;
	o.tangent = v.tangent;
	o.uv = TRANSFORM_TEX(v.uv, _GrassMask);
	return o;
}

TessellationFactors patchConstantFunction (InputPatch<vertexInput, 3> patch)
{
	float3 world = mul(unity_ObjectToWorld, (patch[0].vertex + patch[1].vertex + patch[2].vertex) / 3);
	float dist = dot(_WorldSpaceCameraPos - world, _WorldSpaceCameraPos - world) + .1;
	float t = dist > _DistanceCulling * _DistanceCulling ? 0 : _TessellationUniform;

	TessellationFactors f;
	f.edge[0] = t;
	f.edge[1] = t;
	f.edge[2] = t;
	f.inside = t;
	return f;
}

[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
[UNITY_partitioning("integer")]
[UNITY_patchconstantfunc("patchConstantFunction")]
vertexInput hull (InputPatch<vertexInput, 3> patch, uint id : SV_OutputControlPointID)
{
	return patch[id];
}

[UNITY_domain("tri")]
vertexOutput domain(TessellationFactors factors, OutputPatch<vertexInput, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
{
	vertexInput v;

	#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) v.fieldName = \
		patch[0].fieldName * barycentricCoordinates.x + \
		patch[1].fieldName * barycentricCoordinates.y + \
		patch[2].fieldName * barycentricCoordinates.z;

	MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)
	MY_DOMAIN_PROGRAM_INTERPOLATE(normal)
	MY_DOMAIN_PROGRAM_INTERPOLATE(tangent)
	MY_DOMAIN_PROGRAM_INTERPOLATE(uv)

	return tessVert(v);
}