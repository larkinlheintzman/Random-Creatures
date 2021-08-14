using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSettings : ScriptableObject
{
    public float islandRadius = 1.0f;
    public NoiseLayer[] noiseLayers;

    [SerializeField]
    public bool[] faceEnable = new bool[6]; // enables and disables each terrain face

    [System.Serializable]
    public class NoiseLayer
    {
      public bool enabled = true;
      public bool useFirstLayerAsMask;
      public bool additive;
      public NoiseSettings noiseSettings;
    }
}
