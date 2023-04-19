Shader "CG/Earth"
{
    Properties
    {
        [NoScaleOffset] _AlbedoMap ("Albedo Map", 2D) = "defaulttexture" {}
        _Ambient ("Ambient", Range(0, 1)) = 0.15
        [NoScaleOffset] _SpecularMap ("Specular Map", 2D) = "defaulttexture" {}
        _Shininess ("Shininess", Range(0.1, 100)) = 50
        [NoScaleOffset] _HeightMap ("Height Map", 2D) = "defaulttexture" {}
        _BumpScale ("Bump Scale", Range(1, 100)) = 30
        [NoScaleOffset] _CloudMap ("Cloud Map", 2D) = "black" {}
        _AtmosphereColor ("Atmosphere Color", Color) = (0.8, 0.85, 1, 1)
    }
    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "CGUtils.cginc"

                // Declare used properties
                uniform sampler2D _AlbedoMap;
                uniform float _Ambient;
                uniform sampler2D _SpecularMap;
                uniform float _Shininess;
                uniform sampler2D _HeightMap;
                uniform float4 _HeightMap_TexelSize;
                uniform float _BumpScale;
                uniform sampler2D _CloudMap;
                uniform fixed4 _AtmosphereColor;

                struct appdata
                { 
                    float4 vertex : POSITION;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    fixed4 ver_pos : TEXCOORD0;
                };

                v2f vert (appdata input)
                {
                    v2f output;
                    output.ver_pos = input.vertex;
                    output.pos = UnityObjectToClipPos(input.vertex);
                    return output;
                }

                fixed4 frag (v2f input) : SV_Target
                {                
                    float3 n = normalize(mul(unity_ObjectToWorld, float3(input.ver_pos[0], input.ver_pos[1], input.ver_pos[2])));
                    float3 v = normalize(_WorldSpaceCameraPos - input.ver_pos);
                    float4 l = normalize(_WorldSpaceLightPos0);
                    float3 l1 = float3(l[0], l[1], l[2]);
                    float2 uv = getSphericalUV(input.ver_pos);
                    half4 albedo = tex2D(_AlbedoMap, uv);
                    half4 spec = tex2D(_SpecularMap, uv);
                    bumpMapData bump;
                    bump.normal = n;
                    bump.tangent = normalize(cross(n, float3(0,1,0)));
                    bump.uv = uv;
                    bump.heightMap = _HeightMap;
                    bump.du = _HeightMap_TexelSize.x;
                    bump.dv = _HeightMap_TexelSize.y;
                    bump.bumpScale = _BumpScale/10000;                    
                    float3 bump_normal = getBumpMappedNormal(bump);
                    float3 finalNormal = normalize((1 - spec) * bump_normal + spec * n);
                    float lambert = max(0,dot(n,l));
                    float3 atmosphere = (1 - max(0,dot(n,v))) * sqrt(lambert) * _AtmosphereColor;
                    float4 clouds = tex2D(_CloudMap,uv) * (sqrt(lambert) + _Ambient);
                    fixed3 res = blinnPhong(finalNormal, v, l1, _Shininess, albedo, spec, _Ambient);
                    return fixed4(res + atmosphere + clouds, 0);                    
                }

            ENDCG
        }
    }
}



