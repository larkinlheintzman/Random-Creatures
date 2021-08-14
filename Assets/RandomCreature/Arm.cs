using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : Limb
{

  public float repeatPeriod = 0.1f;
  private float motionEndTime = 0.0f;
  private Vector3 refVelocity = Vector3.zero;

  // public override void IdlePosition()
  // {
  //
  // }

  // Update is called once per frame
  private void FixedUpdate()
  {
    base.FrameUpdate();

    if (initialized)
    {

      if (GameManager.me.inputManager.punchPressed && !inMotion && Time.time > motionEndTime + repeatPeriod)
      {
        if (generator.GetRandomArm() == this && !generator.CheckPunching())
        {
          Vector3 starting = bone.position;
          // Vector3 starting = new Vector3(bone.position.x, attachPoint.transform.position.y, bone.position.z);

          Vector3 yChange = new Vector3(bone.position.x, idleTarget.position.y, bone.position.z);
          Vector3 final = yChange + limbLength*player.forward;

          motion = new Motion(starting, final, generator.equippedBody.transform, Motion.PathType.line, Motion.LookType.normal, motionSpeedCurve, player.forward, layerMask, false);

          inMotion = true;
        }
      }

      if (!inMotion)
      {
        // sidle on up to idle position

        // Vector3 angleOffset = idleMoveScale*(Mathf.Sin(randAngle)*generator.transform.up + Mathf.Cos(randAngle)*generator.transform.right);

        // apply return force
        float distanceScaler = Mathf.Clamp(1.0f/(previousPosition -idleTarget.position).sqrMagnitude, 0.001f, 10.0f);

        Vector3 newPosition = Vector3.SmoothDamp(previousPosition, idleTarget.position + idlePositionOffset, ref refVelocity, positionSmoothTime*distanceScaler);

        // Vector3 newVerticalPosition = Vector3.SmoothDamp(previousPosition, idleTarget.position + idlePositionOffset, ref refVelocity, 0.5f*positionSmoothTime*distanceScaler);

        target.position = newPosition;

      }

      if (inMotion)
      {
        target.position = motion.MotionUpdate(Time.deltaTime);
        target.rotation = motion.RotationUpdate(Time.deltaTime);

        if (motion.complete)
        {
          inMotion = false;
          motionEndTime = Time.time;
        }
      }

      // do something on hit too
    }

  }

}
