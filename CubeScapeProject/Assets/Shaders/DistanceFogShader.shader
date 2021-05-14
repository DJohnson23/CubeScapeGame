Shader "Hidden/DistanceFog"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _FogStart("Fog Start", Float) = 30
        _FogDistance("Fog Distance", Float) = 130
        _FogColor("Fog Color", Color) = (0, 0, 0, 0)
    }
        SubShader
        {
            // No culling or depth
            Cull Off ZWrite Off ZTest Always

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float3 viewVector : TEXCOORD1;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                    o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));

                    return o;
                }

                float4 lerp(float4 a, float4 b, float t) {
                    t = saturate(t);

                    return a + t * (b - a);
                }

                sampler2D _MainTex;
                sampler2D _CameraDepthTexture;
                float _FogStart;
                float _FogDistance;
                float4 _FogColor;

                float4 frag(v2f input) : SV_Target
                {
                    float4 col = tex2D(_MainTex, input.uv);
                    float3 rayOrigin = _WorldSpaceCameraPos;
                    float3 rayDir = normalize(input.viewVector);

                    float nonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, input.uv);
                    float depth = LinearEyeDepth(nonLinearDepth) * length(input.viewVector);

                    if (depth > _FogStart) {
                        float dst = depth - _FogStart;
                        float t = dst / _FogDistance;

                        col = lerp(col, _FogColor, t);
                    }

                    return col;
                }
                ENDCG
            }
        }
}
