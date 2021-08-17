using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordArm : Limb
{

  public float repeatPeriod = 0.1f;
  public float swingAngle = Mathf.PI/2;
  public BoneCollider edgeCollider;
  public ParticleSystem swingParticles;
  public float damage = 2f;
  private float motionEndTime = 0.0f;
  private Vector3 refVelocity = Vector3.zero;

  // public override void IdlePosition()
  // {
  //
  // }

  public override void Initialize(CreatureGenerator generator, int id)
  {
    base.Initialize(generator, id);
    edgeCollider = bone.gameObject.GetComponent<BoneCollider>();
  }

  // Update is called once per frame
  private void FixedUpdate()
  {
    base.FrameUpdate();

    if (initialized)
    {

      if (playerManager.inputManager.punchPressed && !inMotion && Time.time > motionEndTime + repeatPeriod)
      {
        if (true)
        {
          // Vector3 swingDir = generator.swingDirection;

          // Vector3 starting = new Vector3(bone.position.x, attachPoint.transform.position.y, bone.position.z);

          // Vector3 yChange = new Vector3(bone.position.x, idleTarget.position.y, bone.position.z);
          // float dir = 1.0f;
          // if (Random.value > 0.5f)
          // {
          //   dir = -1.0f;
          // }
          float tempyval = Mathf.Sin(swingAngle);
          // float tempxval = Mathf.Cos(swingAngle);
          // Mathf.DeltaAngle(current, target);
          // Vector3 startOffset = new Vector3(Mathf.Sin(swingAngle), 0.0f, Mathf.Cos(swingAngle));
          Quaternion rotation = Quaternion.AngleAxis(swingAngle, Vector3.up);
          Vector3 startOffset = rotation*(-Vector3.right);
          rotation = Quaternion.AngleAxis(-swingAngle, Vector3.up);
          Vector3 stopOffset = rotation*(-Vector3.right);
          // startOffset.Normalize();
          // stopOffset.Normalize();

          Vector3 starting = startOffset*limbLength;
          Vector3 final = stopOffset*limbLength;
          // Vector3 final = player.position + player.TransformVector(stopOffset*limbLength);
          // Vector3 final = generator.equippedBody.transform.position;

          motion = new Motion(starting, final, player, Motion.PathType.arc, Motion.LookType.normal, motionSpeedCurve, player.forward, layerMask, false);

          inMotion = true;
          // play swoosh particles
          swingParticles.Play();
        }
      }

      if (!inMotion)
      {
        Vector3 newPosition = Vector3.SmoothDamp(previousPosition, idleTarget.position + idlePositionOffset, ref refVelocity, positionSmoothTime);
        target.position = newPosition;

        // derpy for now
        bone.rotation = Quaternion.LookRotation(player.up + player.forward, player.up);

        //bone.rotation = stuff;

      }

      if (inMotion)
      {
        target.position = motion.MotionUpdate(Time.deltaTime);
        bone.rotation = motion.RotationUpdate(Time.deltaTime);
        // bone.rotation = target.rotation;

        if (motion.complete)
        {
          inMotion = false;
          swingParticles.Stop();
          motionEndTime = Time.time;
        }

        if (edgeCollider.isHit)
        {
          inMotion = false;
          swingParticles.Stop();
          playerManager.particleContainer.PlayParticle(2, edgeCollider.hitPoint);
          motionEndTime = Time.time;
          Twitch(-bone.forward, twitchScale, twitchRandomScale);

          // particleContainer.PlayParticle(2, transform.position);
          Health health = edgeCollider.other.gameObject.GetComponent<Health>();
          if (health != null)
          {
            health.Damage(damage);
          }
        }
      }

      // do something on hit too
    }

  }

}
