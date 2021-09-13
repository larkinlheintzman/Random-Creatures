using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class musicVizController : MonoBehaviour
{
  public Image barImage;
  private Image[] barImages;
  public int specSize = 128;
  public int vizSpacing = 10;
  public int vizGap = 3;
  public int vizHeight = 10;
  public float[] spectrum;
  // get images to move to music

  // public void OnEnable()
  // {
  //   // make image array to move around
  //   barImages = new Image[specSize];
  //   spectrum = new float[specSize];
  //   for(int i = 0; i < spectrum.Length; i++)
  //   {
  //     barImages[i] = Instantiate(barImage, transform).GetComponent<Image>();
  //     // barImages[i].transform.parent =
  //   }
  // }

  public void Start()
  {
    // make image array to move around
    barImages = new Image[specSize];
    spectrum = new float[specSize];
    for(int i = 0; i < spectrum.Length; i++)
    {
      barImages[i] = Instantiate(barImage, transform).GetComponent<Image>();
      // barImages[i].transform.parent =
    }
  }

  public void Update()
  {
    if (barImages[0] == null)
    {
      barImages = new Image[specSize];
      for(int i = 0; i < spectrum.Length; i++)
      {
        barImages[i] = Instantiate(barImage, transform).GetComponent<Image>();
        // barImages[i].transform.parent =
      }
    }

    spectrum = new float[specSize];
    AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

    for (int i = 0; i < spectrum.Length; i++) {
      // Vector3[] corners = new Vector3[4];
      RectTransform rts = barImages[i].GetComponent<RectTransform>();
      // Vector2 max = rts.anchorMax;
      rts.anchoredPosition = new Vector2(vizSpacing*i + vizGap, 0.0f);
      rts.sizeDelta = new Vector2(vizSpacing + vizGap, vizHeight*spectrum[i]);
      // calc new corners

      // Debug.DrawLine(new Vector3(i-1*vizSpacing, 0, 0), new Vector3(i-1*vizSpacing, vizHeight*spectrum[i], 0), Color.red);
    }

  }
}
