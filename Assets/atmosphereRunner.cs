using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class atmosphereRunner : MonoBehaviour
{

  [HideInInspector]
  public Material atmoMaterial;
  [HideInInspector]
  public Shader atmoShader;
  [HideInInspector]
  public CityStarGenerator generator;
  [Header("atmo settings")]
  public float lightIntensity = 1.0f;
  public Vector4 lightColor = Color.red;
  public Vector3 lightDirection = Vector3.zero;
  public float planetRadius = 0.0f;
  public float atmosphereRadius = 110f;
  public float colorSteps = 10f;
  public float lightSteps = 10f;
  public Vector4 rayleighScattering = new Vector4(0.08f, 0.2f, 0.51f, 0.64f);
  public Vector4 mieScattering = new Vector4(0.01f, 0.9f, 0.0f, 0.8f);
  public float clipThreshold = 0.5f;

  public void SetValues()
  {
    atmoMaterial = gameObject.GetComponent<Renderer>().material;
    // set shader values?
    atmoMaterial = gameObject.GetComponent<Renderer>().sharedMaterial;
    atmoMaterial.SetVector("_PlanetCenter", transform.position);
    atmoMaterial.SetVector("_RayleighScattering", rayleighScattering);
    atmoMaterial.SetVector("_MieScattering", mieScattering);
    atmoMaterial.SetVector("_LightColor", lightColor);
    atmoMaterial.SetFloat("_PlanetRadius", planetRadius);
    atmoMaterial.SetFloat("_AtmosphereRadius", atmosphereRadius);
    atmoMaterial.SetFloat("_Steps", colorSteps);
    atmoMaterial.SetFloat("_LightSteps", lightSteps);
    atmoMaterial.SetFloat("_LightIntensity", lightIntensity);
    atmoMaterial.SetFloat("_ClipThreshold", clipThreshold);


    Light[] suns = FindObjectsOfType<Light>();
    foreach(Light l in suns)
    {
      if (l.type == LightType.Directional)
      {
        //set direction like so?
        atmoMaterial.SetVector("_LightDirection", l.transform.rotation*Vector3.forward);
      }
    }
  }
  public void Start()
  {
    SetValues();
  }


}
