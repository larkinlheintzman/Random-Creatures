using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneCollider : MonoBehaviour
{

    public bool initialized = false;
    public Vector3 hitPoint = Vector3.zero;
    public bool isHit = false;
    public int iFrames = 5;
    public int hitDelayCounter = 0;
    // attached rigidbody which must be there to hit ground
    public Rigidbody rb;
    // attached boneCollider for toggling reasons
    public Collider boneCollider;
    public Collider other; // thing that we hit
    // interaction layer mask bc reasons
    LayerMask layerMask;


    void OnTriggerEnter(Collider col)
    {
      if(initialized && hitDelayCounter == 0)
      {
        if(layerMask == (layerMask | 1 << col.gameObject.layer))
        {
          isHit = true;
          hitDelayCounter = iFrames;
          hitPoint = col.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
          other = col;
          // Debug.Log("hit detected!" + this.ToString());
          // Debug.Log(this);
        }
      }
    }
    // // do sumthing with layer masks!

    public void ClearHit()
    {
      if (hitDelayCounter <= 0)
      {
        isHit = false; // clear hit, called once
        hitPoint = Vector3.zero; // no hit point
        other = null;
      }
    }

    void LateUpdate()
    {
      if (hitDelayCounter > 0)
      {
        hitDelayCounter -= 1;
      }
    }

    public void Initialize(LayerMask layerMask)
    {
      this.initialized = true;
      this.rb = GetComponent<Rigidbody>();
      this.boneCollider = GetComponent<Collider>();
      this.layerMask = layerMask;
      // RedgeDollToggle(false);
    }

    // why do i do these things to myself
    public void RedgeDollToggle(bool redge)
    {
      // whatch these nots right herya
      rb.isKinematic = !redge;
      boneCollider.isTrigger = !redge;

      // print("flipping bone:" + gameObject.ToString() + " " + redge);
    }

    // aw fuck it https://forum.unity.com/threads/fix-ontriggerexit-will-now-be-called-for-disabled-gameobjects-colliders.657205/

}
