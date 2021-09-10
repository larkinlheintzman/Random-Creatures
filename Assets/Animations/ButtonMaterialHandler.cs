using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonMaterialHandler : MonoBehaviour
{

  public Material mainMaterial;
  public Material altMaterial;
  public TMP_Text tmpText;
  public ButtonBehavior btnAnim;
  public bool materialChangedFlag = false;
  public float maxDial = 0.5f;
  public float inflateSpeed = 0.1f;

  private float prevDial;
  private float currDial;

  public void Awake()
  {
    tmpText = GetComponentInChildren<TMP_Text>();
    Animator anim = GetComponent<Animator>();
    btnAnim = anim.GetBehaviour<ButtonBehavior>();
    ChangeMaterial(false); // pick main material at first
    materialChangedFlag = false;
    prevDial = mainMaterial.GetFloat("_FaceDilate");
    currDial = prevDial;
  }

  public void Update()
  {

    if (btnAnim == null || tmpText == null)
    {
      return;
    }

    // initial highlight
    if (btnAnim.highlighted && !materialChangedFlag)
    {
      ChangeMaterial(true);
    }

    if (btnAnim.highlighted && materialChangedFlag)
    {
      // inflate
      currDial = Mathf.Lerp(currDial, maxDial, inflateSpeed);
      altMaterial.SetFloat("_FaceDilate", currDial);
    }

    if (!btnAnim.highlighted && materialChangedFlag)
    {
      // deflate
      currDial = Mathf.Lerp(currDial, prevDial, inflateSpeed);
      altMaterial.SetFloat("_FaceDilate", currDial);
    }

    if (Mathf.Abs(currDial - prevDial) <= 0.01f && materialChangedFlag)
    {
      // swap back
      ChangeMaterial(false);
    }

  }

  public void ChangeMaterial(bool matFlag)
  {
    if (matFlag)
    {
      tmpText.fontMaterial = altMaterial;
      materialChangedFlag = true;
    }
    else
    {
      tmpText.fontMaterial = mainMaterial;
      materialChangedFlag = false;
    }

  }

  public void OnEnable()
  {
    tmpText = GetComponentInChildren<TMP_Text>();
    Animator anim = GetComponent<Animator>();
    btnAnim = anim.GetBehaviour<ButtonBehavior>();
    ChangeMaterial(false); // pick main material at first
    materialChangedFlag = false;
    prevDial = mainMaterial.GetFloat("_FaceDilate");
    currDial = prevDial;
  }

  public void OnDisable()
  {
    ChangeMaterial(false); // pick main material at first
    materialChangedFlag = false;
    prevDial = mainMaterial.GetFloat("_FaceDilate");
    currDial = prevDial;
  }
}
