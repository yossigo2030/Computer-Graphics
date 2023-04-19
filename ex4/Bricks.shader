Shader "CG/Bricks"
{
    Properties
    {
        [NoScaleOffset] _AlbedoMap ("Albedo Map", 2D) = "defaulttexture" {}
        _Ambient ("Ambient", Range(0, 1)) = 0.15
        [NoScaleOffset] _SpecularMap ("Specular Map", 2D) = "defaulttexture" {}
        _Shininess ("Shininess", Range(0.1, 100)) = 50
        [NoScaleOffset] _HeightMap ("Height Map", 2D) = "defaulttexture" {}
        _BumpScale ("Bump Scale", Range(-100, 100)) = 40
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

                struct appdata
                { 
                    float4 vertex   : POSITION;
                    float3 normal   : NORMAL;
                    float4 tangent  : TANGENT;
                    float2 uv       : TEXCOORD0;
                    
                    
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                    float3 worldPosition : TEXTCOORD1;
                    float4 tangent : TEXTCORD2;
                    
                };

                v2f vert (appdata input)
                {
                    v2f output;
                    output.uv = input.uv;
                    output.pos = UnityObjectToClipPos(input.vertex);
                    output.worldPosition = mul(unity_ObjectToWorld, input.vertex);
                    output.normal = mul(unity_ObjectToWorld, input.normal);
                    output.tangent = mul(unity_ObjectToWorld, input.tangent);
                    return output;                    
                }

                fixed4 frag (v2f input) : SV_Target
                {
                    float3 normal = normalize(mul(unity_ObjectToWorld, input.normal));
                    float3 v = normalize(_WorldSpaceCameraPos - input.worldPosition);
                    float4 l = normalize(_WorldSpaceLightPos0);
                    float3 l1 = float3(l[0], l[1], l[2]);
                    fixed4 albedo = tex2D(_AlbedoMap, input.uv);
                    fixed4 spec = tex2D(_SpecularMap, input.uv);
                    
                    bumpMapData bump;
                    bump.normal = normal;
                    bump.tangent = input.tangent;
                    bump.uv = input.uv;
                    bump.heightMap = _HeightMap;
                    bump.du = _HeightMap_TexelSize.x;
                    bump.dv = _HeightMap_TexelSize.y;
                    bump.bumpScale = _BumpScale/10000;                    
                    float3 bump_normal = getBumpMappedNormal(bump);
                    
                    
                    fixed3 res = blinnPhong(bump_normal, v, l1, _Shininess, albedo, spec, _Ambient);
                    return fixed4(res, 0);
                }

            ENDCG
        }
    }
}


/*

Shader "CG/Bricks"
{
    Properties
    {
        [NoScaleOffset] _AlbedoMap ("Albedo Map", 2D) = "defaulttexture" {}
        _Ambient ("Ambient", Range(0, 1)) = 0.15
        [NoScaleOffset] _SpecularMap ("Specular Map", 2D) = "defaulttexture" {}
        _Shininess ("Shininess", Range(0.1, 100)) = 50
        [NoScaleOffset] _HeightMap ("Height Map", 2D) = "defaulttexture" {}
        _BumpScale ("Bump Scale", Range(-100, 100)) = 40
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

                struct appdata
                { 
                    float4 vertex   : POSITION;
                    float3 normal   : NORMAL;
                    float4 tangent  : TANGENT;
                    float2 uv       : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;

                    // We added
                    float2 uv  : TEXCOORD0;
                    float3 normal   : TEXCOORD1;
                    float3 post : TEXCOORD2;
                    float4 tangent : TEXTCORD3;
                };

                v2f vert (appdata input)
                {
                    v2f output;
                    output.pos = UnityObjectToClipPos(input.vertex);
                    output.uv = input.uv;
                    output.normal = mul(unity_ObjectToWorld, input.normal);
                    output.post = mul(unity_ObjectToWorld, input.vertex);
                    output.tangent = mul(unity_ObjectToWorld, input.tangent);
                    return output;
                }

                fixed4 frag (v2f input) : SV_Target
                {
                    float3 v = normalize(_WorldSpaceCameraPos.xyz - input.post); // Camera view direction
                    
                    float3 l = _WorldSpaceLightPos0.xyz; // Light direction
                    
                    float3 n =  normalize(input.normal);

                    bumpMapData bump_map_data;
                    
                    bump_map_data.normal = n;
                    bump_map_data.tangent = normalize(input.tangent);
                    
                    bump_map_data.uv = input.uv;
                    float pixelSizeU =  _HeightMap_TexelSize.x; 
                    float pixelSizeV =  _HeightMap_TexelSize.y;    
                    
                    
                    bump_map_data.heightMap = _HeightMap;
                    
                    bump_map_data.du = pixelSizeU; // may change
                    bump_map_data.dv = pixelSizeV;
                    bump_map_data.bumpScale = _BumpScale/10000;
                    
                    float3 newNormal = getBumpMappedNormal(bump_map_data);
                    
                    fixed3 color = blinnPhong(newNormal,v,l,_Shininess,
                        tex2D(_AlbedoMap,input.uv),tex2D(_SpecularMap,input.uv),_Ambient);

                    
                    return fixed4(color,0);
                }

            ENDCG
        }
    }
}

*/
