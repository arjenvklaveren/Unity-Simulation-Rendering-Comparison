using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderMeshIndirect : MonoBehaviour
{
    public int instanceCount = 1000000;
    public Material material;
    public Mesh mesh;
    public ComputeShader computeShader;

    GraphicsBuffer commandBuffer;
    GraphicsBuffer particlesBuffer;

    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    RenderParams rp;
    const int commandCount = 1;

    float targetParticleHeight = 4.0f;

    void Start()
    {
        commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        particlesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instanceCount, 16);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];
        UpdateBuffers();
    }

    struct Particle
    {
        public Vector4 position;
    };

    void UpdateBuffers()
    {
        rp = new RenderParams(material);
        rp.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
        rp.matProps = new MaterialPropertyBlock();

        Particle[] particles = new Particle[instanceCount];
        for (int i = 0; i < instanceCount; i++)
        {
            float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
            float distance = Random.Range(20.0f, 100.0f);
            float height = Random.Range(-2.0f, 2.0f);
            float size = Random.Range(0.05f, 0.25f);
            particles[i].position = new Vector4(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance, size);
        }
        particlesBuffer.SetData(particles);
        rp.matProps.SetBuffer("particleBuffer", particlesBuffer);

        commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)instanceCount;

        commandBuffer.SetData(commandData);

        computeShader.SetFloat("targetHeight", targetParticleHeight);

        int kernel = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernel, "particleBuffer", particlesBuffer);
    }

    void Update()
    {
        computeShader.SetFloat("time", Time.time);
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.Dispatch(0, Mathf.CeilToInt(instanceCount / 64f), 1, 1);
        Graphics.RenderMeshIndirect(rp, mesh, commandBuffer, 1);
    }

    void OnDestroy()
    {
        commandBuffer?.Release();
        commandBuffer = null;
        particlesBuffer?.Release();
        particlesBuffer = null;
        computeShader = null;
    }
}
