using Unity.Entities;
using UnityEngine;

public class ParticleSpawnerAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public int spawnAmount;
}

class ParticleSpawnerBaker: Baker<ParticleSpawnerAuthoring>
{
    public override void Bake(ParticleSpawnerAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new ParticleSpawnerComponent
        {
            prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
            spawnAmount = authoring.spawnAmount,
            hasSpawned = false,
            random = Unity.Mathematics.Random.CreateFromIndex((uint)Random.Range(1, int.MaxValue))
        });
    }
}

