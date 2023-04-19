
Shader "CG/Water"
{
    Properties
    {
        _CubeMap("Reflection Cube Map", Cube) = "" {}
        _NoiseScale("Texture Scale", Range(1, 100)) = 10 
        _TimeScale("Time Scale", Range(0.1, 5)) = 3 
        _BumpScale("Bump Scale", Range(0, 0.5)) = 0.05
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "CGUtils.cginc"
                #include "CGRandom.cginc"

                #define DELTA 0.01

                // Declare used properties
                uniform samplerCUBE _CubeMap;
                uniform float _NoiseScale;
                uniform float _TimeScale;
                uniform float _BumpScale;

                struct appdata
                { 
                    float4 vertex   : POSITION;
                    float3 normal   : NORMAL;
                    float4 tangent  : TANGENT;
                    float2 uv       : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos      : SV_POSITION;
                    float2 uv       : TEXTCORD1;
                    float3 ver_pos : TEXCOORD2;
                    float3 normal   : TEXTCORD3;
                    float4 tangent  : TEXTCORD4;
                };

                // Returns the value of a noise function simulating water, at coordinates uv and time t
                float waterNoise(float2 uv, float t)
                {
                    return perlin3d(float3(0.5 * uv[0], 0.5 * uv[1], 0.5 * t) + 0.5 *
                        perlin3d(float3(uv[0], uv[1], t)) + 0.2 * perlin3d(float3(2 * uv[0], 2 * uv[1], 2 * t)));
                }

                // Returns the world-space bump-mapped normal for the given bumpMapData and time t
                float3 getWaterBumpMappedNormal(bumpMapData i, float t)
                {
                    float2 uv = i.uv * _NoiseScale;
                    //t = t * _TimeScale;
                    float2 noise = waterNoise(uv,t * _TimeScale);
                    float fp_du = waterNoise(uv + float2(i.du, 0), t * _TimeScale);
                    float fp_dv = waterNoise(uv + float2(0, i.dv), t * _TimeScale);
                    float tu = (fp_du - noise) / i.du;
                    float tv = (fp_dv - noise) / i.dv;
                    float3 norlmalize_nh = normalize(float3(-tu * i.bumpScale, -tv * i.bumpScale, 1));
                    float3 b = normalize(cross(i.tangent,i.normal)); 
                    return normalize(float3(i.tangent * norlmalize_nh[0] + i.normal * norlmalize_nh[2] + b * norlmalize_nh[1]));
                }


                v2f vert (appdata input)
                {
                    v2f output;
                    float displace = (waterNoise(input.uv *_NoiseScale, _Time.y * _TimeScale)) * _BumpScale;
                    output.uv = input.uv;
                    output.normal = input.normal;
                    output.tangent = input.tangent;
                    float4 nDisplace = float4(displace * input.normal, 0);
                    output.pos = UnityObjectToClipPos(input.vertex + nDisplace);
                    output.ver_pos = mul(unity_ObjectToWorld, input.vertex + nDisplace);
                    return output;
                }

                fixed4 frag (v2f input) : SV_Target
                {
                    bumpMapData bump_map_data;
                    bump_map_data.du = DELTA;
                    bump_map_data.dv = DELTA;
                    bump_map_data.bumpScale = _BumpScale;
                    bump_map_data.normal = normalize(mul(unity_ObjectToWorld, normalize(input.normal)));
                    bump_map_data.tangent = normalize(mul(unity_ObjectToWorld, normalize(input.tangent)));
                    bump_map_data.uv = input.uv;
                    float3 n = getWaterBumpMappedNormal(bump_map_data, _Time.y);

                    //float3 n = normalize(input.normal);
                    float3 v = normalize(_WorldSpaceCameraPos.xyz - input.ver_pos);
                    float3 r = 2 * dot(v, n) * n - v;                    
                    float4 reflectedColor = texCUBE(_CubeMap,r);
                    float3 color = (1 - max(0, dot(n, v)) + 0.2) * reflectedColor;                     
                    //float color = (waterNoise(input.uv *_NoiseScale, 0));
                    //color = (color + 1)/ 2;
                    return fixed4(color, 1);


                    
                }

            ENDCG
        }
    }
}
