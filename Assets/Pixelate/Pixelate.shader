Shader "Unlit/Pixelate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Resolution ("Resolution", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 pos		: POSITION;
                float2 uv		: TEXCOORD0;
            };

            struct v2f
            {
                float4 pos		: SV_POSITION;
                float2 uv		: TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Resolution;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
				if (_Resolution >= 1)
					return tex2D(_MainTex, i.uv);
				float2 pixelSize = 1 / _Resolution / _ScreenParams.xy;
				float2 uv = floor(i.uv / pixelSize) * pixelSize;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
