using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class musicBox : MonoBehaviour
{
  public GameObject bar;
  private GameObject[] barSet;
  public int specSize = 256;
  public int specStep = 16; // divsor of spectrum
  public int vizSpacing = 10;
  public int vizGap = 3;
  public int vizHeight = 10;
  public Vector3 baseSize = new Vector3(1.0f, 1.0f, 1.0f);

  public float[] spectrum;
  // get images to move to music

  public void Start()
  {
    Setup();
  }

  public void Setup()
  {
    // make image array to move around
    barSet = new GameObject[specSize/specStep];
    spectrum = new float[specSize];
    for(int i = 0; i < specSize/specStep; i++)
    {
      barSet[i] = Instantiate(bar, transform);
      barSet[i].transform.parent = null;
      barSet[i].name = $"bar_{i}";
      // barSet[i].transform.parent =
    }
  }

  public void Update()
  {
    if (barSet[0] == null || barSet.Length != specSize/specStep)
    {
      Setup();
    }

    spectrum = new float[specSize];
    AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

    for (int i = 0; i < specSize/specStep; i++)
    {
      barSet[i].transform.position = transform.position + transform.TransformVector(new Vector3(i*vizSpacing + vizGap, 0.0f, 0.0f));
      barSet[i].transform.localScale = baseSize + new Vector3(0.0f, spectrum[i*specStep]*vizHeight, 0.0f);
    }

  }
}
