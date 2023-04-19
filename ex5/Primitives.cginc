void updateRay(inout RayHit rayHit, float t, Material material, float3 normal, float3 position)
{
    rayHit.distance = t;
    rayHit.material = material;
    rayHit.normal = normal;
    rayHit.position = position;
}

void intersectSphere(Ray ray, inout RayHit bestHit, Material material, float4 sphere)
{    
    float3 c = sphere.xyz;
    float3 o = ray.origin;
    float3 d = ray.direction;
    float r = sphere.w;
    float A = 1;
    float B = 2 * dot(o - c, d);
    float C = dot(o - c, o - c) - r * r;
    float discriminant = B * B - 4 * A * C;
    float t;
    if(discriminant < 0)
    {
        t = 0;
    }
    else if(discriminant == 0)
    {
        t = -B / (2 * A);
    }
    else
    {
        float2 res = float2((-B + sqrt(discriminant)) / (2 * A), (-B - sqrt(discriminant)) / (2 * A));
        if(res[0] <= 0)
        {
            t = res[1];
        }
        else if(res[1] <= 0)
        {
            t = res[0];
        }
        else
        {
            t = min(res[0], res[1]);
        }
    }
    if (t < bestHit.distance && t > EPS) {
        updateRay(bestHit, t, material, normalize(o + t * d - c), o + t * d);
    }
}


// Checks for an intersection between a ray and a plane
// The plane passes through point c and has a surface normal n
void intersectPlane(Ray ray, inout RayHit bestHit, Material material, float3 c, float3 n)
{
    if ( dot(ray.direction, n) == 0)
    {
        return;
    }
    float t = -dot(ray.origin - c, n) / dot(ray.direction, n);
    if (t <= EPS)
    {
        return;
    }
    if (t < bestHit.distance) {
        updateRay(bestHit, t, material, n, ray.origin + t * ray.direction);
    }
}

// Checks for an intersection between a ray and a plane
// The plane passes through point c and has a surface normal n
// The material returned is either m1 or m2 in a way that creates a checkerboard pattern 
void intersectPlaneCheckered(Ray ray, inout RayHit bestHit, Material m1, Material m2, float3 c, float3 n)
{
    if ( dot(ray.direction, n) == 0)
    {
        return;
    }
    float t = -dot(ray.origin - c, n) / dot(ray.direction, n);
    if (t <= EPS)
    {
        return;
    }
    float3 p = ray.origin + t * ray.direction;
    if (t < bestHit.distance) {
        float3 temp = frac(p);
        float first, second;
        if (abs(p.x) < EPS) {
            first = temp[1];
            first = temp[2];

        }
        else if (abs(p.y) < EPS) {
            first = temp[0];
            first = temp[2];
        }
        else {
            first = temp[0];
            first = temp[1];
        }
        if (first < 0.5f && second >= 0.5f || first >= 0.5f && second < 0.5f) {
            updateRay(bestHit, t, m2, n, p);
        }
        else
        {
            updateRay(bestHit, t, m1, n, p);
        }
        
    }        
}

// Checks for an intersection between a ray and a triangle
// The triangle is defined by points a, b, c
void intersectTriangle(Ray ray, inout RayHit bestHit, Material material, float3 a, float3 b, float3 c)
{
    float3 n = normalize(cross(a - c, b - c));
    if ( dot(ray.direction, n) == 0)
    {
        return;
    }
    float t = -dot(ray.origin - c, n) / dot(ray.direction, n);
    if (t <= EPS)
    {
        return;
    }
    float3 p = ray.origin + t * ray.direction;
    if (t < bestHit.distance && dot(cross(b - a, p - a), n) >= 0 &&
        dot(cross(c - b, p - b), n) >= 0 && dot(cross(a - c, p - c), n) >= 0)
    {
        updateRay(bestHit, t, material, n, p);
    }
}

// Checks for an intersection between a ray and a 2D circle
// The circle center is given by circle.xyz, its radius is circle.w and its orientation vector is n 
void intersectCircle(Ray ray, inout RayHit bestHit, Material material, float4 circle, float3 n)
{
    if (dot(n, ray.direction) == 0) {
        return;
    }
    float t = -dot(ray.origin - circle.xyz, n) / dot(ray.direction, n);
    if (t <= EPS)
    {
        return;
    }
    if (t < bestHit.distance) {
        float3 p = ray.origin + t * ray.direction;
        float pointsOfCircle = sqrt(dot(p - circle.xyz, p - circle.xyz));
        if (pointsOfCircle <= circle.w) {
            updateRay(bestHit, t, material, n, p);
        }
    }
}

// Checks for an intersection between a ray and a cylinder aligned with the Y axis
// The cylinder center is given by cylinder.xyz, its radius is cylinder.w and its height is h
void intersectCylinderY(Ray ray, inout RayHit bestHit, Material material, float4 cylinder, float h)
{
    float A = ray.direction[0] * ray.direction[0] + ray.direction[2] * ray.direction[2];
    float B = 2 * (ray.direction[0] * (ray.origin[0] - cylinder[0]) +
        ray.direction[2] * (ray.origin[2] - cylinder[2]));
    float c1 = (ray.origin[2] - cylinder[2]) * (ray.origin[2] - cylinder[2]);
    float c2 = (cylinder[0] - ray.origin[0]) * (cylinder[0] - ray.origin[0]);
    float c3 = cylinder[3] * cylinder[3];
    float C = c1 + c2 - c3;
    float discriminant = B * B - 4 * A * C;
    float t;
    if(discriminant < 0)
    {
        t = 0;
    }
    else if(discriminant == 0)
    {
        t = -B / (2 * A);
    }
    else
    {
        float2 res = float2((-B + sqrt(discriminant)) / (2 * A), (-B - sqrt(discriminant)) / (2 * A));
        if(res[0] <= 0)
        {
            t = res[1];
        }
        else if(res[1] <= 0)
        {
            t = res[0];
        }
        else
        {
            t = min(res[0], res[1]);
        }
    }
    if (t <= EPS)
    {
        return;
    }
    if (t < bestHit.distance) {
        float3 p = ray.origin + t * ray.direction;
        if (p[1] <= cylinder[1] + h / 2 && p[1] >= cylinder[1] - h / 2) {
            updateRay(bestHit, t, material, normalize(p - float3 (cylinder[0], p[1], cylinder[2])), p);
        }
    }
    intersectCircle(ray, bestHit, material, float4(cylinder[0], cylinder[1] + h / 2, cylinder[2], cylinder[3]), float3(0, 1, 0));
    intersectCircle(ray, bestHit, material, float4(cylinder[0], cylinder[1] - h / 2, cylinder[2], cylinder[3]), float3(0, -1, 0));
}

