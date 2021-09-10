using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetLeg : Limb
{

  private Vector3 previousVelocity = Vector3.zero;
  public float jetMovementScale = 0.15f;
  public float jetTiltScale = 0.5f;
  public float jetTiltSpeed = 0.5f;

  private Vector3 smoothVel = Vector3.zero;
  // private float idleCounter = 0.0f;

  private void FixedUpdate()
  {
    base.FrameUpdate();
    if (initialized)
    {

      // update position
      Vector3 accVector = generator.rb.velocity - previousVelocity;
      target.position = idleTarget.position + jetMovementScale*accVector + idlePositionOffset;
      previousVelocity = generator.rb.velocity;

      // update bone rotation samely
      Quaternion newBoneRotation = Quaternion.FromToRotation(generator.transform.up, generator.transform.up + jetTiltScale*accVector);
      bone.rotation = Quaternion.Lerp(bone.rotation, newBoneRotation, jetTiltSpeed);

    }
  }

}
