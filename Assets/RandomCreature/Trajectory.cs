using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
  public enum TrajType {arc, parabola, line};
  public enum SpeedType {constant, increasing, decreasing, ramping};
  public enum LookType {up, normal, tangent};

  public Transform target;
  public Transform player;
  public Vector3 start = Vector3.zero;
  public Vector3 stop = Vector3.zero;
  public Vector3 previousPosition = Vector3.zero;
  public Vector3 positionUpdate = Vector3.zero;
  public TrajParams trajParams;
  public bool done = false;
  public float progress = 0.0f;

  public Trajectory NewTraj(Vector3 start, Vector3 stop, Transform targetToMove, Transform targetToTrack, TrajParams trajParams)
  {
    this.target = targetToMove;
    this.player = targetToTrack;
    this.start = start;
    this.stop = stop;
    this.trajParams = trajParams;
    switch(trajParams.trajType)
    {
      case TrajType.arc:
        StartCoroutine(RunArcTrajectory());
        break;
      case TrajType.line:
        StartCoroutine(RunLineTrajectory());
        break;
      case TrajType.parabola:
        StartCoroutine(RunArcTrajectory());
        break;
      default:
        StartCoroutine(RunArcTrajectory());
        break;
    }
    return this;
  }

  public IEnumerator RunArcTrajectory()
  {
    // move target along path
    progress = 0.0f;
    done = false;

    while (!done)
    {
      Vector3 localOffset = Vector3.Slerp(start, stop, progress);
      target.position = MapVec(localOffset);
      RunRotation();
      progress += trajParams.speed*Time.fixedDeltaTime;
      if (progress >= 1.0f) done = true;
      // record previous position
      previousPosition = target.position;
      yield return null;
    }
  }

  public IEnumerator RunLineTrajectory()
  {
    // move target along path
    progress = 0.0f;
    done = false;
    while (!done)
    {
      Vector3 localOffset = Vector3.Lerp(start, stop, progress);
      target.position = MapVec(localOffset);
      RunRotation();
      progress += trajParams.speed*Time.fixedDeltaTime;
      if (progress >= 1.0f) done = true;
      yield return null;
    }
  }

  // do parabolic arc
  public IEnumerator RunParabolicTrajectory()
  {

    // move target along path
    progress = 0.0f;
    done = false;

    while (!done)
    {
      float parabolicT = progress * 2 - 1;
      //start and end are not level, gets more complicated
      Vector3 travelDirection = stop - start;
      Vector3 levelDirection = stop - new Vector3(start.x, stop.y, start.z);
      Vector3 right = Vector3.Cross(travelDirection, levelDirection);
      Vector3 up = trajParams.normal;
      Vector3 localOffset = start + progress * travelDirection;
      localOffset += ((-parabolicT * parabolicT + 1) * trajParams.radius) * up.normalized;
      target.position = MapVec(localOffset);
      RunRotation();
      progress += trajParams.speed*Time.fixedDeltaTime;
      if (progress >= 1.0f) done = true;
      // record previous position
      previousPosition = target.position;
      yield return null;
    }
  }

  private void RunRotation()
  {
    switch (trajParams.lookType)
    {
      case LookType.up:
        target.rotation = Quaternion.LookRotation(player.forward, player.up);
        break;
      case LookType.normal:
        target.rotation = Quaternion.LookRotation(target.position - player.position, player.up);
        break;
      case LookType.tangent:
        target.rotation = Quaternion.LookRotation(target.position - previousPosition, player.up);
        break;
      default:
        target.rotation = Quaternion.LookRotation(target.position - player.position, player.up);
        break;
    }
  }

  private Vector3 MapVec(Vector3 localPoint)
  {
    return player.TransformPoint(localPoint);
  }

  // private Vector3 SampleCircle(float t)
  // {
  //   // t = 0 gives start
  //   // t = 1 gives stop point
  //   Quaternion
  //   Vector3 result = center + radius*(Mathf.Sin(t*angle)*forward + Mathf.Cos(t*angle)*right);
  //
  //   // Debug.Log("arc angle: " + angle);
  //   // Debug.Log("arc progress: " + t);
  //
  //   return result;
  //
  // }

}
