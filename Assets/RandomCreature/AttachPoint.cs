using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AttachPoint : MonoBehaviour
{
  public Limb[] allowedLimbs;
  public bool restrictLimbs = false;
  // public void Initialize(CreatureGenerator gen)
  // {
  //   parent = gen;
  // }

  public Limb[] GetLimbTypes()
  {
    return allowedLimbs;
  }

}
