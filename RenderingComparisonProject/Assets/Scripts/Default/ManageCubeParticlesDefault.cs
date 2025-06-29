using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageCubeParticlesDefault : MonoBehaviour
{
    public int spawnAmount;
    public GameObject prefab;

    List<GameObject> particles = new List<GameObject>();

    void Start()
    {
        SpawnParticles();
    }

    void SpawnParticles()
    {
        Random.InitState(12345);

        for (int i = 0; i < spawnAmount; i++)
        {
            float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
            float distance = Random.Range(20.0f, 100.0f);
            float height = Random.Range(-2.0f, 2.0f);
            float size = Random.Range(0.05f, 0.25f);
            Vector3 position = new Vector3(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance);

            GameObject particle = Instantiate(prefab, this.transform);
            particle.transform.localScale = new Vector3(size, size, size);
            particle.transform.position = position;

            particles.Add(particle);
        }
    }
    void Update()
    {
        MoveParticles();
    }

    public static Vector2 Rotate2D(Vector2 v, float r)
    {
        float s = Mathf.Sin(r);
        float c = Mathf.Cos(r);
        return new Vector2(v.x * c - v.y * s,v.x * s + v.y * c);
    }

    void MoveParticles()
    {
        for(int i = 0; i < particles.Count; i++)
        {
            GameObject particle = particles[i];

            Vector2 inPos2D = new Vector2(particle.transform.position.x, particle.transform.position.z);
            float dist = inPos2D.magnitude; 

            float rotationSpeed = (Time.deltaTime * (105.0f - dist)) * 0.01f;
            Vector2 outPos2D = Rotate2D(inPos2D, rotationSpeed);

            float targetHeight = 4.0f;
            float particleHeight = Mathf.Sin(dist * 0.1f + (Time.time * 1.75f)) * targetHeight + ((100 - dist) * 0.1f);

            Vector3 outPos3D = new Vector3(outPos2D.x, particleHeight, outPos2D.y);
            particle.transform.position = outPos3D;
        }
    }
}
