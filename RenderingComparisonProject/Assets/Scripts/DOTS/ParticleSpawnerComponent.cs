using Unity.Entities;
using Unity.Mathematics;

public struct ParticleSpawnerComponent : IComponentData
{
    public Entity prefab;
    public int spawnAmount;
    public bool hasSpawned;
}
