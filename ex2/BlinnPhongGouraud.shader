Shader "CG/BlinnPhongGouraud"
{
    Properties
    {
        _DiffuseColor ("Diffuse Color", Color) = (0.14, 0.43, 0.84, 1)
        _SpecularColor ("Specular Color", Color) = (0.7, 0.7, 0.7, 1)
        _AmbientColor ("Ambient Color", Color) = (0.05, 0.13, 0.25, 1)
        _Shininess ("Shininess", Range(0.1, 50)) = 10
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

                // From UnityCG
                uniform fixed4 _LightColor0; 

                // Declare used properties
                uniform fixed4 _DiffuseColor;
                uniform fixed4 _SpecularColor;
                uniform fixed4 _AmbientColor;
                uniform float _Shininess;

                struct appdata
                { 
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float4 color : COLOR0;
                };


                v2f vert (appdata input)
                {
                    v2f output;
                    output.pos = UnityObjectToClipPos(input.vertex);
                    float3 normaInWorldCord = normalize(mul(unity_ObjectToWorld, input.normal));
                    float4 normalIn4D = float4(normaInWorldCord, 0);
                    float4 normalisedV = normalize(float4(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, input.vertex), 0));
                    float4 normalisedL = normalize(_WorldSpaceLightPos0);
                    fixed4 ambient = _AmbientColor * _LightColor0;
                    float diffuseCoefficient = max(dot(normalisedL, normalIn4D),0);
                    float4 diffuse = diffuseCoefficient * (_DiffuseColor * _LightColor0);
                    float4 h = normalize((normalisedL + normalisedV)/2);
                    float specularCoefficient = max(dot(normalIn4D, h),0);
                    float4 specular = pow(specularCoefficient, _Shininess) * _SpecularColor * _LightColor0;
                    output.color = ambient + diffuse + specular;
                    return output;
                }


                fixed4 frag (v2f input) : SV_Target
                {
                    return input.color;
                }

            ENDCG
        }
    }
}
