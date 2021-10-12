using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleArm : Limb
{

  public SpringJoint rope;
  public LayerMask grappleLayerMask;

  public float releaseDelay = 3.0f;
  public float releaseReelSpeed = 5.0f;
  public float grappledEnergyConsumption = 0.25f;
  public float rof = 2; // good ol rate of fire per sec
  public float maxGrabRange = 20.0f;
  public float aimSpeed = 0.1f;
  public Vector2 aimingOffset = new Vector2(-0.5f, 0.5f);

  private LineRenderer ropeRenderer;
  public bool shotOnCooldown = false;
  public float grappledAt = -1.0f;
  private float shotCooldownCounter = 0.0f;
  private Vector3 refVelocity = Vector3.zero;
  private Transform grappledTransform;
  private Vector3 grappledTransformOffset = Vector3.zero;

  private void FixedUpdate()
  {
    base.FrameUpdate();

    if (initialized)
    {
      // UpdateAimTargetPoint();

      if (rope == null)
      {
        // add spring joint to generator under our name
        rope = generator.gameObject.AddComponent<SpringJoint>();
        rope.damper = 0.5f;
        rope.spring = 500f;
        rope.autoConfigureConnectedAnchor = false;
        rope.anchor = generator.transform.TransformVector(idleTarget.position);
        ropeRenderer = GetComponent<LineRenderer>();

      }

      if (playerManager.inputManager.aiming)
      {
        target.position = idleTarget.position + aimingOffset[0]*generator.transform.up;
        Quaternion oldRotation = bone.rotation;
        bone.LookAt(generator.playerManager.aimTarget);
        Debug.DrawLine(bone.position, bone.position + bone.forward*100f, Color.white, 0.1f);
        Quaternion newRotation = bone.rotation;
        bone.rotation = Quaternion.Lerp(oldRotation, newRotation, aimSpeed);
      }
      else
      {
        // climb if available
        if (pos.isGrounded && ((target.position - transform.position).sqrMagnitude > stepRadius*stepRadius))
        {
          target.position = pos.worldPosition;
          bone.rotation = Quaternion.LookRotation(pos.groundNormal, player.forward);
        }
        else if (!pos.isGrounded)
        {
          target.position = idleTarget.position + aimingOffset[1]*generator.transform.up;
          bone.rotation = Quaternion.LookRotation(generator.transform.forward, generator.transform.up);
        }
      }
      // handle shootin'
      if (playerManager.inputManager.shootPressed && !shotOnCooldown && generator.energy.hasGas)
      {
        if (generator.GrappleTurnAlternator() == id)
        {

          energyConsumption = 0.0f;
          playerManager.particleContainer.PlayParticle(3, bone.position + bone.forward, transform);
          // raycast forward, get spring component off generator, set anchors to be limb offsets
          RaycastHit hitPoint = new RaycastHit();
          if (Physics.Raycast(bone.position + 0.1f*bone.forward, bone.forward, out hitPoint, maxGrabRange, grappleLayerMask))
          {
            energyConsumption = grappledEnergyConsumption;
            grappledTransform = hitPoint.transform;
            Vector3 vec = bone.position + bone.forward*hitPoint.distance - hitPoint.transform.position;
            grappledTransformOffset = hitPoint.transform.InverseTransformDirection(vec).normalized*vec.magnitude;
            // grappledTransformOffset = bone.position + bone.forward*hitPoint.distance - hitPoint.transform.position;

            rope.connectedAnchor = bone.position + bone.forward*hitPoint.distance;
            rope.maxDistance = hitPoint.distance;
            rope.minDistance = 0.0f;

            ropeRenderer.enabled = true;
            ropeRenderer.SetPositions(new Vector3[2] {target.position, rope.connectedAnchor});
            grappledAt = Time.time;
          }
          else
          {
            grappledAt = -1.0f;
          }

          shotOnCooldown = true;
          shotCooldownCounter += 1/rof;

          // shock gun angle
          bone.rotation = bone.rotation*Quaternion.AngleAxis(-25f, Vector3.right);
          Twitch(-bone.forward, twitchScale, twitchRandomScale);
        }
      }
      // grappled
      if (grappledAt > -1.0f)
      {
        // move connected anchor to correc point
        rope.connectedAnchor = grappledTransform.position + grappledTransform.rotation*grappledTransformOffset;
        // move rope to correc points
        ropeRenderer.SetPositions(new Vector3[2] {target.position, rope.connectedAnchor});
      }

      // gas o meter check
      if (!generator.energy.hasGas)
      {
        Release();
      }

      if (shotOnCooldown)
      {
        if (shotCooldownCounter > 0.0f)
        {
          shotCooldownCounter -= Time.fixedDeltaTime;
        }
        else
        {
          shotCooldownCounter = 0.0f;
          shotOnCooldown = false;
        }
      }

      if (grappledAt <= -1.0f)
      {
        // holster that there grabby
        rope.connectedAnchor = bone.position;
        rope.maxDistance = maxGrabRange;
        rope.minDistance = 0.0f;

        ropeRenderer.SetPositions(new Vector3[0]);
        ropeRenderer.enabled = false;
      }

      // do space bar charge here ---------------------------
      if (playerManager.inputManager.jumpPressed && grappledAt > -1.0f)
      {
        StartCoroutine(SpaceBarChargeRelease(id));
      }

    }
  }

  // public float lastEndTime = 0.0f;
  public IEnumerator SpaceBarChargeRelease(int offset)
  {
    // lastEndTime = Time.time + releaseDelay*(offset + 1);
    while(grappledAt > -1.0f)
    {
      if (!playerManager.inputManager.jumpPressed)
      {
        // lastEndTime = 0.0f;
        Release();
        yield break;
      }
      else
      {
        // still holding space bar, reel in some line if there's room
        if (rope.maxDistance > 5.0f) rope.maxDistance = rope.maxDistance - releaseReelSpeed*Time.fixedDeltaTime;
      }

      yield return null;
    }
    // lastEndTime = 0.0f;
    // Release();
    // if (playerManager.inputManager.jumpPressed)
    // {
    // }
  }

  public void Release()
  {
    // lert ger
    grappledAt = -1.0f;
    energyConsumption = 0.0f;
  }


  public override void Uninstall()
  {
    Destroy(rope);
    base.Uninstall();
  }

}
