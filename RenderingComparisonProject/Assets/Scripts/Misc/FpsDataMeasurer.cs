using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsDataMeasurer : MonoBehaviour
{
    float[] deltaTimes = new float[100];
    void Start()
    {
        StartCoroutine(MeasureFPS());
    }

    IEnumerator MeasureFPS()
    {
        int currentMeasureCount = 0;
        yield return new WaitForSeconds(3);

        while(currentMeasureCount < 100)
        {
            float currentDeltaTime = Time.deltaTime;
            deltaTimes[currentMeasureCount] = currentDeltaTime;
            currentMeasureCount++;
            yield return null;
        }

        Debug.Log(GetAverageFPS());
    }

    float GetAverageFPS()
    {
        float totalDeltaTime = 0;
        for(int i = 0; i < deltaTimes.Length; i++)
        {
            totalDeltaTime += deltaTimes[i];
        }
        totalDeltaTime /= 100;
        return 1.0f / totalDeltaTime;
    }
}
