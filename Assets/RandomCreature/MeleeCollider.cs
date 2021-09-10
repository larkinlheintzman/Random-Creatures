using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCollider : BoneCollider
{

  public Limb parentLimb;

  public void Awake()
  {
    parentLimb = GetComponentInParent<Limb>(); // find parent limb
  }

  // for rigidbody contacts
  public virtual void OnCollisionEnter(Collision col)
  {
    if(initialized && hitDelayCounter == 0)
    {
      if(layerMask == (layerMask | 1 << col.gameObject.layer))
      {
        isHit = true;
        hitDelayCounter = iFrames;
        hitPoint = col.GetContact(0).point;
        other = col.collider;

        // stop swing motion on limb
        if (parentLimb is SwordArm)
        {
          (parentLimb as SwordArm).DoHit(col);
        }
      }
    }
  }


}
