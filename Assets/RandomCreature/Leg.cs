using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Leg : Limb
{

  // Update is called once per frame
  private void FixedUpdate()
  {
    base.FrameUpdate();
    if (initialized)
    {
      AngleToes(player.up); // stay pointing the same way. need player.groundNormal here
      if (!generator.CheckStepping(id) && CheckNeedStep() && !inMotion)
      {

        pos = FindGround();
        if (pos.isGrounded)
        {
          // Debug.Log("position found was grounded");
          Vector3 starting = target.position - player.position;
          Vector3 final = pos.worldPosition - player.position;// + groundOffset*transform.up;
          // put fuckin better angling stuff into motion
          TrajParams pars = new TrajParams();
          pars.trajType = Trajectory.TrajType.parabola;
          traj.NewTraj(starting, final, target, generator.transform, new TrajParams());
          // motion = new Motion(starting, final, player, Motion.PathType.parabola, Motion.LookType.player, motionSpeedCurve, Vector3.up, layerMask, false);

          inMotion = true;
        }
        else
        {
          // bad!
          // Debug.Log("position found was not grounded");
          target.position = pos.worldPosition; // if not grounded, just move directly to point
        }

      }
      else if (inMotion)
      {
        // Debug.Log("position found was grounded");
        // target.position = motion.MotionUpdate(Time.deltaTime);
        // target.rotation = motion.RotationUpdate(Time.deltaTime);

        if (traj.done)
        {
          inMotion = false;
          // AngleToes(pos.groundNormal); // stay pointing the same way
        }
      }
    }
  }

  void AngleToes(Vector3 normal)
  {
    bone.rotation = Quaternion.LookRotation(player.forward, normal);
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
    if (Vector3.Dot(target.position - player.position, transform.position - player.position) < 0.0f)
    {
      // Debug.Log("stepping due to wrong sides");
      return true;
    }

    return false;
  }

  public virtual LimbPosition FindGround()
  {
    LimbPosition outputPosition = new LimbPosition();
    Vector3 groundNormal = Vector3.zero;

    // -----------------------------
    bool groundFlag = false;
    Vector3[] spherePts = PointsOnSphere(pointNumber);

    Vector3 currentGroundPosition = Vector3.zero;
    float currentMinDistance = 100f; // will only become a problem when legs > 100f in length

    foreach (Vector3 value in spherePts.Take(raycastNumber)) // only evaluate a few downward facing ones
    {
      if (drawRaycasts) Debug.DrawLine(transform.position, transform.position + limbLength*value, Color.green,0.1f);
      if (Vector3.Dot(value, transform.forward) >= 0.0f)
      {
        if (drawRaycasts) Debug.DrawLine(transform.position, transform.position + limbLength*value + 0.05f*Vector3.right, Color.blue,0.1f);
        RaycastHit hitPoint = new RaycastHit(); // efficiency bro
        if (Physics.Raycast(transform.position, value, out hitPoint, limbLength, layerMask))
        {
          // if (drawRaycasts) Debug.DrawLine(transform.position, transform.position + limbLength*value - 0.05f*Vector3.right, Color.white,0.1f);
          if (drawRaycasts) Debug.DrawLine(transform.position, transform.position + value*hitPoint.distance - 0.05f*Vector3.right, Color.white,0.1f);
          // float mappedDist = GroundDistanceMapping(transform.position + value*hitPoint.distance, localOffset);
          if (hitPoint.distance < currentMinDistance)
          {
            groundFlag = true; // found ground
            // currentGroundPosition = transform.position + value*(hitPoint.distance);
            currentGroundPosition = transform.position + value*hitPoint.distance;
            groundNormal = hitPoint.normal; // average normals
            currentMinDistance = hitPoint.distance;

              // // check normal is close to vertical
              // if (Vector3.Dot(hitPoint.normal, player.up) > 0.0f)
              // {
              //   groundFlag = true; // found ground
              //   currentGroundPosition = idleTarget.position + value*(hitPoint.distance);
              //   groundNormal = hitPoint.normal; // average normals
              //   currentMinDistance = hitPoint.distance;
              // }
          }
        }
      }
    }
    // -----------------------------

    if (groundFlag)
    {
      // found ground
      outputPosition.worldPosition = currentGroundPosition;
      outputPosition.isGrounded = true;
      outputPosition.groundNormal = groundNormal;

      return outputPosition;

    }
    outputPosition.worldPosition = AirPosition();
    // outputPosition.worldPosition = Vector3.zero;
    outputPosition.isGrounded = false;
    outputPosition.groundNormal = groundNormal; // no normal in air!

    return outputPosition;
  }

}
