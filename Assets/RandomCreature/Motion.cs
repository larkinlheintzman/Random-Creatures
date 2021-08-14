using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motion
{
  // different arcs between points
  [HideInInspector]
  public enum PathType {parabola, arc, point, line};
  public enum LookType {normal, player}; // which way to face the transform

  public Vector3 finalPosition;
  public Vector3 startingPosition;
  public Vector3 currentPosition;
  public Vector3 currentRotation; // vector pointing in swoarad direction
  public Vector3 motionDirection; // vector that points in direction of arc (if parabolic)
  public Vector3 motionPlane; // vector that defines normal to flat planes that transform moves in
  public AnimationCurve motionSpeedCurve;
  public Transform player;
  public Transform target; // thing that does the motion/angling
  public LayerMask layerMask; // thing
  public float progress;
  public float speed;
  public bool complete = false;
  public PathType pathType;
  public LookType lookType;
  public bool checkCollisions;

  public Motion InitMotion()
  {
    return this;
  }

  public Motion(Vector3 starting,
  Vector3 final,
  Transform playerTransform,
  PathType pathType,
  LookType lookType,
  AnimationCurve motionSpeedCurve,
  Vector3 motionDirection,
  LayerMask layerMask,
  bool checkCollisions)
  {
    this.checkCollisions = checkCollisions;
    this.player = playerTransform;
    this.startingPosition = starting;
    this.finalPosition = final;

    this.currentPosition = starting;
    this.motionDirection = motionDirection;
    this.motionPlane = Vector3.Cross(final - starting, player.forward);

    this.pathType = pathType;
    this.lookType = lookType;

    this.speed = 0.0f; // units along path per sec
    this.progress = 0.0f;
    this.complete = false;
    this.layerMask = layerMask;
    this.motionSpeedCurve = motionSpeedCurve;

  }

  // public List<Motion> BuildMotion()
  // {
  //
  // }

  // move along path
  public Vector3 MotionUpdate(float timeElapsed)
  {
    if (!complete)
    {
      Vector3 start = player.TransformVector(startingPosition);
      Vector3 final = player.TransformVector(finalPosition);

      // do some speed ramping stuff
      speed = motionSpeedCurve.Evaluate(progress);

      progress += speed * timeElapsed; // progress is parametric 0 to 1
      if (progress >= 1) progress = 1.0f;

      // only go if raycast is clear
      Vector3 tempNextPosition = Vector3.zero;

      // parabolic path
      if (pathType == PathType.parabola)
      {
        tempNextPosition = SampleParabola(start + player.position, final + player.position, 0.5f, progress, player.TransformVector(motionDirection));
      }

      // arc path
      else if(pathType == PathType.arc)
      {
        tempNextPosition = SampleCircle(start + player.position, final + player.position, player.position, progress, player.up);
      }

      // motion is point
      else if(pathType == PathType.point)
      {
        tempNextPosition = final; // pretty stupid but does some things
      }

      // line path
      else if(pathType == PathType.line)
      {
        Vector3 unitVector = final - start;
        float totalDist = unitVector.magnitude;
        unitVector.Normalize();
        tempNextPosition = player.position + start + unitVector*progress*totalDist;
      }

      // regardless of type of motion, check collisions
      float distToNextPoint = (tempNextPosition - currentPosition).magnitude;
      if (checkCollisions)
      {

        RaycastHit hitPoint = new RaycastHit();
        if (!Physics.Raycast(currentPosition, tempNextPosition - currentPosition, out hitPoint, distToNextPoint, layerMask))
        {
          currentPosition = tempNextPosition;
        }
        else
        {
          Debug.Log("hit something en route");
          currentPosition = hitPoint.transform.position; // line right up to er
          // and we're done with the stride
          complete = true;
          progress = 0.0f;
        }
      }
      else
      {
        currentPosition = tempNextPosition;
      }

      // check if done
      if (progress >= 1.0f)
      {
        complete = true;
        progress = 0.0f;
      }
    }
    // target.position = currentPosition;
    return currentPosition;
    // return target;
  }

  public Quaternion RotationUpdate(float timeElapsed)
  {
    if (!complete)
    {
      if (lookType == LookType.normal)
      {
        if (pathType == PathType.parabola)
        {
          // point at normal
          Vector3 tangent = SampleParabola(startingPosition, finalPosition, 1.0f, progress+0.01f, player.TransformVector(motionDirection)) -
          SampleParabola(startingPosition, finalPosition, 1.0f, progress-0.01f, player.TransformVector(motionDirection));
          motionPlane = Vector3.Cross(finalPosition - startingPosition, player.forward);
          return Quaternion.LookRotation(-Vector3.Cross(tangent, motionPlane), player.up);
        }

        if (pathType == PathType.arc)
        {
          // point at normal
          Vector3 tangent = SampleCircle(player.TransformVector(startingPosition) + player.position, player.TransformVector(finalPosition) + player.position, player.position, progress + 0.01f, player.up) -
          SampleCircle(player.TransformVector(startingPosition) + player.position, player.TransformVector(finalPosition) + player.position, player.position, progress - 0.01f, player.up);
          return Quaternion.LookRotation(-Vector3.Cross(tangent, player.up), player.up);
          // motionPlane = Vector3.Cross(finalPosition - startingPosition, player.forward);
          // return Quaternion.LookRotation(-Vector3.Cross(tangent, motionPlane), player.up);
        }

      }
      else if (lookType == LookType.player)
      {
        // point at player forward
        return player.rotation;
      }
    }
    return player.rotation;
  }

  Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t, Vector3 outDirection)
  {
    float parabolicT = t * 2 - 1;
    //start and end are not level, gets more complicated
    Vector3 travelDirection = end - start;
    Vector3 levelDirection = end - new Vector3(start.x, end.y, start.z);
    Vector3 right = Vector3.Cross(travelDirection, levelDirection);
    Vector3 up = outDirection;
    Vector3 result = start + t * travelDirection;
    result += ((-parabolicT * parabolicT + 1) * height) * up.normalized;
    return result;
  }

  Vector3 SampleCircle(Vector3 start, Vector3 end, Vector3 center, float t, Vector3 localup)
  {
    // t = 0 gives start
    // t = 1 gives end point
    Vector3 forward = start - center;
    Vector3 right = Vector3.Cross(localup, forward);

    float radius = forward.magnitude;
    float angle = (Vector3.Angle(start - center, end - center))*(Mathf.PI/180f);

    forward.Normalize();
    right.Normalize();

    Vector3 result = center + radius*(Mathf.Sin(t*angle)*forward + Mathf.Cos(t*angle)*right);

    // Debug.Log("arc angle: " + angle);
    // Debug.Log("arc progress: " + t);

    return result;

  }
}
