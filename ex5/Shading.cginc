// Implements an adjusted version of the Blinn-Phong lighting model
float3 blinnPhong(float3 n, float3 v, float3 l, float shininess, float3 albedo)
{
    float3 diffuse = max(0, dot(n, l)) * albedo;
    float3 h = normalize(l + v);    
    float3 specural = pow(max(0, dot(n, h)), shininess) * 0.4;
    return diffuse + specural;
}

// Reflects the given ray from the given hit point
void reflectRay(inout Ray ray, RayHit hit)
{
    ray.direction = normalize(2 * dot(-ray.direction, hit.normal) * hit.normal + ray.direction);
    ray.energy = ray.energy * hit.material.specular;
    ray.origin = hit.position + EPS * hit.normal;
}

// Refracts the given ray from the given hit point
void refractRay(inout Ray ray, RayHit hit)
{
    float3 i = ray.direction;   
    float3 n = hit.normal;
    float eta;
    if (dot(n, i) > 0) {
        n = -n;
        eta = hit.material.refractiveIndex;
    }
    else
    {
        eta = 1 / hit.material.refractiveIndex;
    }    
    ray.origin = hit.position - EPS * n;
    float3 c1 = abs(dot(n, i));    
    float3 c2 = sqrt(1 - eta * eta * (1 - c1 * c1));    
    float3 t = eta * i + (eta * c1 - c2) * n;    
    ray.direction = normalize(t);
}

// Samples the _SkyboxTexture at a given direction vector
float3 sampleSkybox(float3 direction)
{
    float theta = acos(direction.y) / -PI;
    float phi = atan2(direction.x, -direction.z) / -PI * 0.5f;
    return _SkyboxTexture.SampleLevel(sampler_SkyboxTexture, float2(phi, theta), 0).xyz;
}