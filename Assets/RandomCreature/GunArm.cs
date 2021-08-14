using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunArm : Limb
{

  public Projectile bulletMold;
  public float launchForce = 100f;
  public float angleSmoothStep = 0.5f;
  public Vector2 aimingOffset = new Vector2(-0.5f, 0.5f);

  public float rof = 2; // good ol rate of fire per sec
  public bool shotOnCooldown = false;
  public float shotCooldownCounter = 0.0f;
  public float aimSpeed = 0.1f;

  private Vector3 refVelocity = Vector3.zero;

  // Update is called once per frame
  private void FixedUpdate()
  {
    base.FrameUpdate();

    if (initialized)
    {
      OrbitCamera orbitCam = generator.cameraTransform.gameObject.GetComponent<OrbitCamera>();
      // target.LookAt(orbitCam.aimTarget);
      if (InputManager.instance.aiming)
      {
        Vector3 newPosition = Vector3.SmoothDamp(target.position,  idleTarget.position + aimingOffset[0]*generator.transform.up, ref refVelocity, positionSmoothTime);
        target.position = newPosition;
        Quaternion oldRotation = bone.rotation;
        bone.LookAt(orbitCam.aimTarget);
        Debug.DrawLine(bone.position, bone.position + bone.forward*100f, Color.white, 0.1f);
        Quaternion newRotation = bone.rotation;
        bone.rotation = Quaternion.Lerp(oldRotation, newRotation, aimSpeed);

      }
      else
      {
        Vector3 newPosition = Vector3.SmoothDamp(target.position,  idleTarget.position + aimingOffset[1]*generator.transform.up, ref refVelocity, positionSmoothTime);
        target.position = newPosition;
        bone.rotation = Quaternion.LookRotation(generator.transform.forward, generator.transform.up);
      }
      // handle shootin'
      if (InputManager.instance.shootPressed && !shotOnCooldown)
      {
        GameManager.me.particleContainer.PlayParticle(3, bone.position + bone.forward, transform);
        Projectile bullet = Instantiate(bulletMold, bone.position + bone.forward, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        bullet.Fire(bone.forward, layerMask);

        shotOnCooldown = true;
        shotCooldownCounter += 1/rof;

        // shock gun angle
        bone.rotation = bone.rotation*Quaternion.AngleAxis(-25f, Vector3.right);

        Twitch(-bone.forward, twitchScale, twitchRandomScale);
      }
      else if (shotOnCooldown)
      {
        if (shotCooldownCounter > 0.0f)
        {
          shotCooldownCounter -= Time.deltaTime;
        }
        else
        {
          shotCooldownCounter = 0.0f;
          shotOnCooldown = false;
        }
      }
    }
  }

}
