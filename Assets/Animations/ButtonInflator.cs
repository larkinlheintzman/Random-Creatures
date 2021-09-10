using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonInflator : MonoBehaviour
{

  public TMP_Text tmpText;
  public ButtonBehavior btnAnim;
  public float maxDial = 0.5f; // character dialation
  public float minDial = 0.1f;
  public float maxSpace = 1.0f; // character spacing
  public float minSpace = -10.0f;
  public float frequency = 0.5f;
  public float damping = 0.5f;
  public float mass = 10.0f;

  // scaling terms, controller goes from 0.0f to 1.0f
  private float dialScaling = 0.0f;
  private float dialOffset = 0.0f;
  private float spaceScaling = 0.0f;
  private float spaceOffset = 0.0f;

  // controller terms
  private float currDial = 0.0f;
  private float currVel = 0.0f; // inflate velocity
  private float currAcc = 0.0f; // inflate acceleration
  private float kd = 0.0f;
  private float kp = 0.0f;
  private float g = 0.0f;

  public void Awake()
  {
    tmpText = GetComponentInChildren<TMP_Text>();
    Animator anim = GetComponent<Animator>();
    btnAnim = anim.GetBehaviour<ButtonBehavior>();
    tmpText.fontMaterial.SetFloat("_FaceDilate", minDial);
    currDial = minDial;

    kp = (6f*frequency)*(6f*frequency)*0.25f;
    kd = 4.5f*frequency*damping;

    dialScaling = (maxDial - minDial)/1.0f;
    dialOffset = minDial;
    spaceScaling = (maxSpace - minSpace)/1.0f;
    spaceOffset = minSpace;
  }

  public void Update()
  {

    kp = (6f*frequency)*(6f*frequency)*0.25f;
    kd = 4.5f*frequency*damping;

    // get controller constants
    float dt = Time.fixedDeltaTime;
    g = 1 / (1 + kd*dt + kp*dt*dt);
    float ksg = kp*g;
    float kdg = (kd + kp*dt)*g;

    if (btnAnim.highlighted)
    {
      currAcc = ((1.0f - currDial) * ksg + (0.0f - currVel) * kdg)/mass;
      currVel += currAcc;
      // inflate
      currDial += currVel;
      tmpText.fontMaterial.SetFloat("_FaceDilate", dialScaling*currDial + dialOffset);
      tmpText.characterSpacing = spaceScaling*currDial + spaceOffset;
    }

    if (!btnAnim.highlighted)
    {
      currAcc = ((0.0f - currDial) * ksg + (0.0f - currVel) * kdg)/mass;
      currVel += currAcc;
      // deflate
      currDial += currVel;
      tmpText.fontMaterial.SetFloat("_FaceDilate", dialScaling*currDial + dialOffset);
      tmpText.characterSpacing = spaceScaling*currDial + spaceOffset;
    }

  }

  public void OnEnable()
  {
    tmpText = GetComponentInChildren<TMP_Text>();
    Animator anim = GetComponent<Animator>();
    btnAnim = anim.GetBehaviour<ButtonBehavior>();
    tmpText.fontMaterial.SetFloat("_FaceDilate", minDial);
    currDial = minDial;
  }

  public void OnDisable()
  {
    tmpText.fontMaterial.SetFloat("_FaceDilate", minDial);
    currDial = minDial;
  }
}
