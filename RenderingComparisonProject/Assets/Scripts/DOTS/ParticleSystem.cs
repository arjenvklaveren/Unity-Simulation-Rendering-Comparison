using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

[BurstCompile]
public partial struct ParticleSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new ParticleMoveJob 
        { 
            deltaTime = SystemAPI.Time.DeltaTime, 
            elapsedTime = (float)SystemAPI.Time.ElapsedTime 
        }
        .ScheduleParallel();
    }

    [BurstCompile]
    public partial struct ParticleMoveJob : IJobEntity
    {
        public float2 rotate2D(float2 v, float r)
        {
            float s, c;
            math.sincos(r, out s, out c);
            v = new float2(v.x * c - v.y * s, v.x * s + v.y * c);
            return v;
        }

        public float deltaTime;
        public float elapsedTime;

        public void Execute(ref LocalTransform transform, ref ParticleComponent particle)
        {
            float2 inPos2D = new float2(transform.Position.x, transform.Position.z);
            float dist = math.length(inPos2D);
            
            float rotationSpeed = (deltaTime * (105.0f - dist)) * 0.01f;
            float2 outPos2D = rotate2D(inPos2D, rotationSpeed);

            float targetHeight = 4.0f;
            float particleHeight = math.sin(dist * 0.1f + (elapsedTime * 1.75f)) * targetHeight + ((100 - dist) * 0.1f);

            float3 outPos3D = new float3(outPos2D.x, particleHeight, outPos2D.y);
            transform.Position = outPos3D;
        }
    }
}
