using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordArm : Limb
{

  public float repeatPeriod = 0.1f;
  public float impactForce = 500.0f;
  public float swingAngle = Mathf.PI/2;
  public MeleeCollider edgeCollider;
  public ParticleSystem swingParticles;
  private float motionEndTime = 0.0f;
  private Vector3 refVelocity = Vector3.zero;
  private Quaternion previousRotation = new Quaternion();

  public override void Initialize(CreatureGenerator generator, int id, int limbIndex)
  {
    base.Initialize(generator, id, limbIndex);
    edgeCollider = bone.gameObject.GetComponent<MeleeCollider>();
  }

  // Update is called once per frame
  private void FixedUpdate()
  {
    base.FrameUpdate();

    if (initialized)
    {

      // work out swing direction
      Vector3 swingDir = player.rotation*Vector3.forward - previousRotation*Vector3.forward;

      if (Vector3.Magnitude(swingDir) <= 0.001)
      {
        // not rotating, pick dir
        if(Random.value > 0.5f) swingDir = player.right;
        else swingDir = -player.right;
      }
      swingDir = Vector3.Normalize(swingDir);

      if (playerManager.inputManager.punchPressed && !inMotion && Time.time > motionEndTime + repeatPeriod)
      {

        Debug.DrawLine(player.position + player.rotation*Vector3.forward, player.position + player.rotation*Vector3.forward + swingDir, Color.blue, 2.0f);

        Vector3 starting = new Vector3(limbLength*Mathf.Sin(swingAngle), 0.0f, limbLength*Mathf.Cos(swingAngle));
        Vector3 final = new Vector3(limbLength*Mathf.Sin(-swingAngle), 0.0f, limbLength*Mathf.Cos(-swingAngle));

        // FUCK quaternions
        Quaternion swingRotation = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(swingDir, player.forward));
        starting = swingRotation*starting;
        final = swingRotation*final;

        Debug.DrawLine(player.position, player.position + final, Color.red, 2.0f);
        Debug.DrawLine(player.position, player.position + starting, Color.green, 2.0f);

        traj.NewTraj(starting, final, target, player, new TrajParams());

        inMotion = true;
        // play swoosh particles
        swingParticles.Play();

        // make edge collider into non trigger
        edgeCollider.boneCollider.isTrigger = false;
      }

      if (!inMotion)
      {
        target.position = idleTarget.position + idlePositionOffset;
        bone.rotation = Quaternion.LookRotation(player.up + player.forward, player.up);
      }

      if (inMotion)
      {
        // target.position = traj.MotionUpdate(Time.deltaTime);
        // bone.rotation = traj.RotationUpdate(Time.deltaTime);

        if (traj.done)
        {
          inMotion = false;
          swingParticles.Stop();
          edgeCollider.boneCollider.isTrigger = true;
          motionEndTime = Time.time;
        }
      }
      previousRotation = player.rotation;
    }
  }

  public void DoHit(Collision col)
  {
    inMotion = false;
    swingParticles.Stop();
    edgeCollider.boneCollider.isTrigger = true;
    print(col.collider.gameObject.name);
    if (col.contactCount > 0)
    {
      if (col == null) return;
      print($"contact point {col.GetContact(0).point}");
      playerManager.particleContainer.PlayParticle(2, col.GetContact(0).point);
    }
    motionEndTime = Time.time;
    Twitch(-bone.forward, twitchScale, twitchRandomScale);

    // if we hit something with a health bar, tick it
    Health otherHealth = col.collider.gameObject.GetComponent<Health>();
    if (otherHealth != null)
    {
      otherHealth.Damage(damage);
    }

    // put heat on thing we hit
    Rigidbody hitRb = col.gameObject.GetComponent<Rigidbody>();
    if (hitRb != null)
    {
      hitRb.AddForce(Vector3.Normalize(col.impulse)*impactForce);
    }
  }

}
