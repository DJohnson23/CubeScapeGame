Shader "Hidden/WaterEffectShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _SeaLevel("Sea Level", Float) = 4
        _WaterColor("Water Color", Color) = (0, 0, 1)
        _ColorBlock("Color Block", Range(0, 1)) = 0.9
        _SightDist("Sight Distance", Float) = 5
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

                float3 lerp(float3 a, float3 b, float t) {
                    t = saturate(t);

                    return a * (1 - t) + b * t;
                }

                sampler2D _MainTex;
                sampler2D _CameraDepthTexture;
                float _SeaLevel;
                float3 _WaterColor;
                float _ColorBlock;
                float _SightDist;

                float3 ApplyColor(float3 col, float depth) {

                    float3 newCol;
                    newCol = col * (1 - _ColorBlock) + _ColorBlock * _WaterColor;
                    newCol = lerp(newCol, _WaterColor, depth / _SightDist);

                    return newCol;
                }

                float4 frag(v2f input) : SV_Target
                {
                    float4 col = tex2D(_MainTex, input.uv);
                    float3 rayOrigin = _WorldSpaceCameraPos;
                    float3 rayDir = normalize(input.viewVector);

                    float nonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, input.uv);
                    float depth = LinearEyeDepth(nonLinearDepth) * length(input.viewVector);

                    float dstToWater = (rayOrigin.y - _SeaLevel) / -rayDir.y;

                    bool underwater = rayOrigin.y < _SeaLevel;
                    bool rayHitWater = dstToWater >= 0 && dstToWater < depth;

                    if (rayHitWater) {

                        if (underwater) {
                            depth = abs(dstToWater);
                            dstToWater = 0;
                        }

                        float depthDiff = depth - dstToWater;

                        col.rgb = ApplyColor(col.rgb, depthDiff);
                    }
                    else if (underwater) {
                        col.rgb = ApplyColor(col.rgb, depth);
                    }

                    return col;
                }
                ENDCG
            }
        }
}
