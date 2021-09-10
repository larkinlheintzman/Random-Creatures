using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajParams
{
  public float radius = 1.0f;
  public float speed = 1.0f;
  public Vector3 normal = Vector3.up;
  public Trajectory.SpeedType speedType = Trajectory.SpeedType.constant;
  public Trajectory.TrajType trajType = Trajectory.TrajType.arc;
  public Trajectory.LookType lookType = Trajectory.LookType.normal;

  // public TrajParams(float radius = 1.0f,
  //                   float speed = 1.0f,
  //                   Vector3 normal = Vector3.up,
  //                   Trajectory.SpeedType speedType = Trajectory.SpeedType.constant,
  //                   Trajectory.TrajType trajType = Trajectory.TrajType.arc,
  //                   Trajectory.LookType lookType = Trajectory.LookType.normal)
  // {
  //   this.radius = radius;
  //   this.speed = speed;
  //   this.normal = normal;
  //   this.speedType = speedType;
  //   this.trajType = trajType;
  //   this.lookType = lookType;
  // }
}
