using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator
{
    [HideInInspector]
    public ShapeSettings settings;
    [HideInInspector]
    public NoiseFilter[] noiseFilters;

    public ShapeGenerator(ShapeSettings settings)
    {
      this.settings = settings;
      noiseFilters = new NoiseFilter[settings.noiseLayers.Length];
      for (int i = 0; i < noiseFilters.Length; i++)
      {
        noiseFilters[i] = new NoiseFilter(settings.noiseLayers[i].noiseSettings);
      }
    }

    public Vector3 CalculatePointOnPlanet(Vector3 pointOnUnitSphere)
    {

      float firstLayerValue = 0.0f;
      float elevation = 0;

      if (noiseFilters.Length > 0)
      {
        firstLayerValue = noiseFilters[0].Evaluate(pointOnUnitSphere);
        if (settings.noiseLayers[0].enabled)
        {
          elevation = firstLayerValue;
        }
      }

      for (int i = 0; i < noiseFilters.Length; i++)
      {
        if (settings.noiseLayers[i].enabled)
        {
          float mask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
          elevation += noiseFilters[i].Evaluate(pointOnUnitSphere) * mask;
        }
      }
      return pointOnUnitSphere * (1 + elevation);
    }

    // just copied from above shhhhh
    public float CalculateNoise(Vector3 point)
    {

      float firstLayerValue = 0.0f;
      float elevation = 0;

      if (noiseFilters.Length > 0)
      {
        firstLayerValue = noiseFilters[0].Evaluate(point);
        if (settings.noiseLayers[0].enabled)
        {
          elevation = firstLayerValue;
        }
      }

      for (int i = 0; i < noiseFilters.Length; i++)
      {
        if (settings.noiseLayers[i].enabled)
        {
          float mask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
          elevation += noiseFilters[i].Evaluate(point) * mask;
        }
      }
      return elevation;
    }

    public float CalculateLayerNoise(Vector3 point, List<int> layerRange)
    {
      float layerValue = 0.0f;
      float elevation = 0;

      if (noiseFilters.Length > 0)
      {
        foreach(int layer in layerRange)
        {
          layerValue = noiseFilters[layer].Evaluate(point);
          if (settings.noiseLayers[layer].enabled)
          {
            if (settings.noiseLayers[layer].additive)
            {
              elevation += layerValue;
            }
            else
            {
              elevation -= layerValue;
            }
          }
        }
      }
      return elevation;
    }

}
