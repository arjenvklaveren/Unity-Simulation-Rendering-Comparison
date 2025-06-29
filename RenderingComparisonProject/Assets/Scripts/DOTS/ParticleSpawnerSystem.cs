using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

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

        if (spawner.ValueRO.hasSpawned) return;

        var prefab = spawner.ValueRO.prefab;
        int spawnAmount = spawner.ValueRO.spawnAmount;

        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        state.Dependency = new ParticleSpawnJob
        {
            prefab = prefab,
            baseSeed = 123456u,
            ecb = ecb
        }.ScheduleParallel(spawnAmount, 64, state.Dependency);


        spawner.ValueRW.hasSpawned = true;
    }

    [BurstCompile]
    public partial struct ParticleSpawnJob : IJobFor
    {
        public Entity prefab;
        public uint baseSeed;
        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute(int index)
        {
            // Use the same random instance with different seeds
            var rand = new Random(math.hash(new uint2(baseSeed, (uint)index)));

            Entity newEntity = ecb.Instantiate(index, prefab);

            float angle = rand.NextFloat(0f, math.PI * 2f);
            float distance = rand.NextFloat(20f, 100f);
            float height = rand.NextFloat(-2f, 2f);
            float size = rand.NextFloat(0.05f, 0.25f);

            float3 position = new float3(math.sin(angle) * distance, height, math.cos(angle) * distance);
            var transform = LocalTransform.FromPositionRotationScale(position, quaternion.identity, size);

            ecb.AddComponent(index, newEntity, new ParticleComponent());
            ecb.SetComponent(index, newEntity, transform);
        }
    }
}
