using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour
{

  // just body things
  private Vector3 previousPosition;
  private CreatureGenerator generator;

  private bool initialized = false;
  private Vector3 smoothVel = Vector3.zero;
  [HideInInspector]
  public Vector3 idlePositionOffset = Vector3.zero;


  public CapsuleCollider mainCollider;
  public Transform limbIdleTransform;
  public Transform bodyPart;
  public float tiltScale = 0.1f;
  public float idleMoveScale = 0.1f;
  [Range(0,1)]
  public float nervousness = 0.01f;
  [Range(0,1)]
  public float positionSmoothTime = 0.1f;
  [Range(0,1)]
  public float tiltSpeed = 0.3f;
  public float motionMass = 0.25f;
  public float motionDrag = 2.0f;
  public ParticleSystem bodyParticles; // impact particles or env interaction

  public void Initialize(CreatureGenerator gen)
  {

    generator = gen; // copy over generator stuff
    previousPosition = generator.transform.position;

    gameObject.layer = generator.gameObject.layer;
    limbIdleTransform.gameObject.layer = generator.gameObject.layer;

    initialized = true;

  }

  public Transform GetLimbIdleTransform()
  {
    // Debug.Log(limbIdleTransform);
    return limbIdleTransform;
  }

  public void FixedUpdate()
  {

    if (initialized)
    {
      // draw line
      Debug.DrawLine(previousPosition, transform.position, Color.white, 1.0f);

      float distanceScaler = Mathf.Clamp(1.0f/(previousPosition - generator.transform.position).sqrMagnitude, 0.001f, 10.0f);

      // apply return force
      Vector3 newPosition = Vector3.SmoothDamp(previousPosition, generator.transform.position + idlePositionOffset, ref smoothVel, positionSmoothTime*distanceScaler);

      // apply return force
      Vector3 newVerticalPosition = Vector3.SmoothDamp(previousPosition, generator.transform.position + idlePositionOffset, ref smoothVel, 0.5f*positionSmoothTime*distanceScaler);


      transform.position = new Vector3(newPosition.x, newVerticalPosition.y, newPosition.z);
      previousPosition = transform.position;
      // previousVelocity = generator.rb.velocity;

      limbIdleTransform.position = transform.position - idlePositionOffset;
      limbIdleTransform.rotation = transform.rotation;

      UpdateIdlePosition();

    }
  }

  public float idleMoveTime = 0.2f;
  private Vector3 idlePositionTarget = Vector3.zero;
  private Vector3 idleVel = Vector3.zero;
  public void UpdateIdlePosition()
  {
    // :)
    if (Random.value <= nervousness) idlePositionTarget = idleMoveScale*Limb.GetRandomSpherical();

    // idlePositionOffset = Vector3.Lerp(idlePositionOffset, idlePositionTarget, idleMoveSpeed);
    idlePositionOffset = Vector3.SmoothDamp(idlePositionOffset, idlePositionTarget, ref idleVel, idleMoveTime);
  }

  public CapsuleCollider GetBodyCollider()
  {
    return mainCollider;
  }

}
