#ifndef CG_UTILS_INCLUDED
#define CG_UTILS_INCLUDED

#define PI 3.141592653

// A struct containing all the data needed for bump-mapping
struct bumpMapData
{ 
    float3 normal;       // Mesh surface normal at the point
    float3 tangent;      // Mesh surface tangent at the point
    float2 uv;           // UV coordinates of the point
    sampler2D heightMap; // Heightmap texture to use for bump mapping
    float du;            // Increment size for u partial derivative approximation
    float dv;            // Increment size for v partial derivative approximation
    float bumpScale;     // Bump scaling factor
};


// Receives pos in 3D cartesian coordinates (x, y, z)
// Returns UV coordinates corresponding to pos using spherical texture mapping
float2 getSphericalUV(float3 pos)
{
    float r = sqrt(pos[0] * pos[0] + pos[1] * pos[1] + pos[2] * pos[2]);
    float teta = atan2(pos[2], pos[0]);
    float fi = acos(pos[1] / r);    
    return float2(0.5 + teta/(2*PI), 1 - fi/PI);
}

// Implements an adjusted version of the Blinn-Phong lighting model
fixed3 blinnPhong(float3 n, float3 v, float3 l, float shininess, fixed4 albedo, fixed4 specularity, float ambientIntensity)
{
    fixed4 ambient1 = ambientIntensity * albedo;
    fixed3 ambient = fixed3(ambient1.x, ambient1.y, ambient1.z);    
    fixed4 diffuse1 = max(0, dot(n,l)) * albedo;
    fixed3 diffuse = fixed3(diffuse1.x, diffuse1.y, diffuse1.z);
    float3 h = normalize((normalize(l) + normalize(v))/2);
    fixed4 specular1 = pow(max(0, dot(n,h)), shininess) * specularity;
    fixed3 specular = fixed3(specular1.x, specular1.y, specular1.z);
    return ambient + diffuse + specular;
}

// Returns the world-space bump-mapped normal for the given bumpMapData
float3 getBumpMappedNormal(bumpMapData i)
{
    

    
    half4 fp = tex2D(i.heightMap, i.uv);
    half4 fp_du = tex2D(i.heightMap, i.uv + float2(i.du, 0));
    half4 fp_dv = tex2D(i.heightMap, i.uv + float2(0, i.dv));
    float3 tu = float3(1, 0, (fp_du[0] - fp[0])/i.du);
    float3 tv = float3(0, 1, (fp_dv[0] - fp[0])/i.dv);    
    float3 nh = cross(tv, tu);
    float3 norlmalize_nh = normalize(float3(-nh[0] * i.bumpScale, -nh[1] * i.bumpScale, 1));
    
    float3 b = normalize(cross(i.tangent, i.normal));
    return normalize(float3(i.tangent * norlmalize_nh[0] + i.normal * norlmalize_nh[2] + b * norlmalize_nh[1]));
}


#endif // CG_UTILS_INCLUDED
