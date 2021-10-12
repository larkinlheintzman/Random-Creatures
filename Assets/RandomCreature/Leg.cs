using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Leg : Limb
{

  public float stepSpeedScaler = 2f;
  // Update is called once per frame
  private void FixedUpdate()
  {
    base.FrameUpdate();
    if (initialized)
    {
      // AngleToes(player.up); // stay pointing the same way. need player.groundNormal here
      // if (!generator.CheckStepping(id) && CheckNeedStep() && !inMotion)
      if (CheckNeedStep() && !inMotion && !generator.CheckStepping())
      {
        if (pos.isGrounded)
        {
          Vector3 starting = player.InverseTransformPoint(target.position);
          Vector3 final = player.InverseTransformPoint(pos.worldPosition);
          // put fuckin better angling stuff into motion
          TrajParams pars = new TrajParams();
          pars.trajType = Trajectory.TrajType.arc;
          pars.speed = stepSpeedScaler;
          traj.NewTraj(starting, final, target, player, pars);

          AngleToes(pos.groundNormal);
          Debug.DrawLine(target.position, target.position + pos.groundNormal*3f, Color.red, 0.25f);

          inMotion = true;
        }
        else
        {
          target.position = pos.worldPosition; // if not grounded, just move directly to point
          AngleToes(pos.groundNormal);
        }
        // angle toes either way

      }
      else if (inMotion)
      {
        if (traj.done)
        {
          inMotion = false;
          // play impact particles
          playerManager.particleContainer.PlayParticle(2, target.position);
        }
      }
      else if (!inMotion)
      {
        // dont need to step and not stepping, stick foot down
        smoothTarget.position = target.position;
        if (pos.isGrounded) AngleToes(pos.groundNormal);
      }
    }
  }

  void AngleToes(Vector3 normal)
  {
    // but IF the forward and normal kind of line up it gives a fucky direction,
    bone.rotation = Quaternion.LookRotation(player.forward, normal.normalized);
    // bone.rotation = Quaternion.FromToRotation(-bone.right, transform.forward);
  }

  bool CheckNeedStep()
  {

    if (Vector3.Distance(target.position, transform.position) >= stepRadius)
    {
      // Debug.Log("stepping due to distance");
      return true;
    }

    // figure out better method to find air feet
    if (!LimbGrounded() && !inMotion)
    {
      // Debug.Log("stepping due to lack of ground");
      return true;
    }

    // // if feet are on wrong side of body lol
    // if (Vector3.Dot(target.position - player.position, transform.position - player.position) < 0.0f)
    // {
    //   Debug.Log("stepping due to wrong sides");
    //   return true;
    // }

    return false;
  }

}
