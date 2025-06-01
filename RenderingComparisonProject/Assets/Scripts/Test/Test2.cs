using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    public int instanceCount = 10;
    public Material material;
    public Mesh mesh;


    GraphicsBuffer commandBuffer;
    GraphicsBuffer positionsBuffer;

    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    const int commandCount = 1;

    RenderParams rp;

    void Start()
    {
        commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        positionsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instanceCount, 16);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];
        UpdateBuffers();
    }

    void UpdateBuffers()
    {
        rp = new RenderParams(material);
        rp.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
        rp.matProps = new MaterialPropertyBlock();

        Vector4[] positions = new Vector4[instanceCount];
        for (int i = 0; i < instanceCount; i++)
        {
            float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
            float distance = Random.Range(20.0f, 100.0f);
            float height = Random.Range(-2.0f, 2.0f);
            float size = Random.Range(0.05f, 0.25f);
            positions[i] = new Vector4(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance, size);
        }
        positionsBuffer.SetData(positions);
        rp.matProps.SetBuffer("positionBuffer", positionsBuffer);

        commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)instanceCount;

        commandBuffer.SetData(commandData);
    }

    void Update()
    {
        Graphics.RenderMeshIndirect(rp, mesh, commandBuffer, 1);
    }

    void OnDestroy()
    {
        commandBuffer?.Release();
        commandBuffer = null;
        positionsBuffer?.Release();
        positionsBuffer = null;
    }
}
