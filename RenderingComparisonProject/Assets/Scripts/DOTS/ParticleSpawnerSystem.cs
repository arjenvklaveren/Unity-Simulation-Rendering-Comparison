using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;

[BurstCompile]
public partial struct ParticleSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ParticleSpawnerComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity spawnerEntity = SystemAPI.GetSingletonEntity<ParticleSpawnerComponent>();
        RefRW<ParticleSpawnerComponent> spawner = SystemAPI.GetComponentRW<ParticleSpawnerComponent>(spawnerEntity);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        if (spawner.ValueRO.hasSpawned) return;

        var random = spawner.ValueRW.random;

        for(int i = 0; i < spawner.ValueRO.spawnAmount; i++)
        {
            Entity newEntity = ecb.Instantiate(spawner.ValueRO.prefab);

            float angle = random.NextFloat(0.0f, math.PI2);
            float distance = random.NextFloat(20.0f, 100.0f);
            float height = random.NextFloat(-2.0f, 2.0f);
            float size = random.NextFloat(0.05f, 0.25f);

            float3 position = new float3(math.sin(angle) * distance, height, math.cos(angle) * distance);

            ecb.AddComponent(newEntity, new ParticleComponent
            {
                moveSpeed = 10
            });

            ecb.SetComponent(newEntity, LocalTransform.FromPositionRotationScale(position, quaternion.identity, size));
        }

        spawner.ValueRW.hasSpawned = true;
        ecb.Playback(state.EntityManager);
    }


    public partial struct ParticleSpawnJob : IJobFor
    {
        public void Execute(int index)
        {
            throw new System.NotImplementedException();
        }
    }
}
