using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : Limb
{

  public float repeatPeriod = 0.1f;
  private float motionEndTime = 0.0f;
  private Vector3 refVelocity = Vector3.zero;

  // Update is called once per frame
  private void FixedUpdate()
  {
    base.FrameUpdate();

    if (initialized)
    {

      if (playerManager.inputManager.punchPressed && !inMotion && Time.time > motionEndTime + repeatPeriod)
      {
        if (generator.GetRandomArm() == this && !generator.CheckPunching())
        {
          Vector3 starting = bone.position;
          // Vector3 starting = new Vector3(bone.position.x, attachPoint.transform.position.y, bone.position.z);

          Vector3 yChange = new Vector3(bone.position.x, idleTarget.position.y, bone.position.z);
          Vector3 final = yChange + limbLength*player.forward;

          traj.NewTraj(starting, final, target, generator.transform, new TrajParams());
          // motion = new Motion(starting, final, generator.equippedBody.transform, Motion.PathType.line, Motion.LookType.normal, motionSpeedCurve, player.forward, layerMask, false);

          inMotion = true;
        }
      }

      if (!inMotion)
      {
        // sidle on up to idle position
        target.position = idleTarget.position + idlePositionOffset;

      }

      if (inMotion)
      {

        if (traj.done)
        {
          inMotion = false;
          motionEndTime = Time.time;
        }
      }

      // do something on hit too
    }

  }

}
