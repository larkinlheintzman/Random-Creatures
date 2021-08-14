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

      // update position smoothally
      Vector3 accVector = generator.rb.velocity - previousVelocity;
      // accVector.Normalize
      // idleCounter += (Random.value + 1.0f)*Time.deltaTime;
      // Vector3 idleOffset = 0.1f*generator.equippedBody.transform.up*Mathf.Sin(idleCounter);
      Vector3 targetPosition = idleTarget.position + jetMovementScale*accVector + idlePositionOffset;

      // Vector3 newVerticalPosition = Vector3.SmoothDamp(previousPosition, targetPosition, ref smoothVel, 0.5f*positionSmoothTime);

      Vector3 newPosition = Vector3.SmoothDamp(previousPosition, targetPosition, ref smoothVel, positionSmoothTime);

      // target.position = new Vector3(newPosition.x, newVerticalPosition.y, newPosition.z);
      target.position = newPosition;

      previousVelocity = generator.rb.velocity;


      // update bone rotation samely
      Quaternion newBoneRotation = Quaternion.FromToRotation(generator.transform.up, generator.transform.up + jetTiltScale*accVector);

      bone.rotation = Quaternion.Lerp(bone.rotation, newBoneRotation, jetTiltSpeed);

    }
  }

}
