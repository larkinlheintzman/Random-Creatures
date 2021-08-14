using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public float strength = 1.0f;

    [Range(1,8)]
    public int numLayers = 1;
    [Range(0,1)]
    public float persistence = 0.5f;

    public float roughness = 2.0f;
    public float baseRoughness = 1.0f;
    public Vector3 center;
    public float minValue;
}
