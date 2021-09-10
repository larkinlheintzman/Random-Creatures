using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingAnimator : MonoBehaviour
{

  private TMP_Text text;
  private float updateRate = 3.0f;
  private float updateCounter = 0.0f;

  public void OnEnable()
  {
    text = GetComponent<TMP_Text>();
    StartCoroutine(StartLoadingAnim());
  }

  public IEnumerator StartLoadingAnim()
  {
    // fiddle with text
    if (updateCounter > updateRate)
    {
      string txt = text.text;
      int dotCount = (txt.Length - txt.Replace(".", "").Length);
      if (dotCount < 3)
      {
        txt = txt + ".";
      }
      else
      {
        txt = txt.Replace(".","");
      }
      text.text = txt;
      updateCounter = 0.0f;
    }
    else
    {
      updateCounter += Time.fixedDeltaTime;
    }

    yield return null;
  }

  // public void Update()
  // {
  //
  // }

}
