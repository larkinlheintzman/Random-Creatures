using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneCollider : MonoBehaviour
{

  // attached rigidbody which must be there to hit ground
    public bool initialized = false;
    public Vector3 hitPoint = Vector3.zero;
    public bool isHit = false;
    public Rigidbody rb;
    public Collider other; // thing that we hit
    public Collider boneCollider;
    public int iFrames = 3;
    public int hitDelayCounter = 0;
    public LayerMask layerMask;

    // for kinematic contacts
    public virtual void OnTriggerEnter(Collider col)
    {
      if(initialized && hitDelayCounter == 0)
      {
        if(layerMask == (layerMask | 1 << col.gameObject.layer))
        {
          isHit = true;
          hitDelayCounter = iFrames;
          hitPoint = col.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
          other = col;
        }
      }
    }

    public virtual void ClearHit()
    {
      if (hitDelayCounter <= 0)
      {
        isHit = false; // clear hit, called once
        hitPoint = Vector3.zero; // no hit point
        other = null;
      }
    }

    public virtual void LateUpdate()
    {
      if (hitDelayCounter > 0)
      {
        hitDelayCounter -= 1;
      }
    }

    public virtual void Initialize(LayerMask layerMask)
    {
      this.initialized = true;
      this.rb = GetComponent<Rigidbody>();
      this.rb.useGravity = false;
      this.boneCollider = GetComponent<Collider>();
      this.layerMask = layerMask;
    }

    // why do i do these things to myself
    public virtual void RedgeDollToggle(bool redge)
    {
      // whatch these nots right herya
      rb.isKinematic = !redge;
      boneCollider.isTrigger = !redge;
    }

}
