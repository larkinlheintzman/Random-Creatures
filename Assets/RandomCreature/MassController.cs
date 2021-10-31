using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MassController : MonoBehaviour
{

  bool initalized = false;

  public Rigidbody rb;
  CharacterInputs input;
  public CreatureGenerator generator;
  public Manager playerManager;

  public Vector3 currentPosition;
  public Vector3 currentVelocity;
  public Vector3 jumpVelocity;
  public Vector3 previousVelocity = Vector3.zero;
  public Vector3 previousPosition = Vector3.zero;
  public Vector3 swingDirection = Vector3.zero;
  public Vector3 previousForward = Vector3.zero;
  public Vector3 localUp = Vector3.up;
  public Vector3 localDown = Vector3.down; // planet side
  public Quaternion desiredLookRotation = Quaternion.Euler(0f,0f,0f);
  public float accelerationTilt = 100.0f;

  public bool mouseRotationMode = false;

  public float rbDrag;
  public float rbMass;

  public float ctrlFrequency = 3f;
  public float ctrlDamping = 1f;
  public float torqueFrequency = 3f;
  public float torqueDamping = 1f;
  public float swingTurnMult = 1f;
  [Range(0f,1f)]
  public float restoringForce = 1.0f;
  private Vector3 lastMappedMoveLook = Vector3.zero;
  private Vector3 lastMoveInput = Vector3.zero;

  // bools to keep track
  public float jumpDelayCount = 10.0f; // maximum possible jump charge
  public float jumpDelayCounter = 0.0f; // counter for delay

  public float playerSpeedMult = 1.8f;
  public float playerAirSpeedMult = 1.1f;
  public float playerRunMult = 2.5f;
  public float playerJumpSpeed = 20.0f;
  public float groundedMaxDistance = 1.05f; // set more better
  public float rotationSpeed = 0.3f;
  [Header("jumping params")]
  public Vector3 jumpBias = Vector3.zero;
  public float jumpCrouchScaler = 0.07f;
  public float jumpHeightScaler = 0.25f;

  private int layerMask;

  private void Awake()
  {
    lastMappedMoveLook = transform.forward;
  }

  public void Initialize(CreatureGenerator gen, Rigidbody rigidbod)
  {

    // copy all es shit
    rb = rigidbod;
    rbDrag = rb.drag;
    rbMass = rb.mass;
    rb.useGravity = false;
    rb.isKinematic = false;

    generator = gen;
    layerMask = gen.layerMask;

    initalized = true;
    playerManager = gen.playerManager;
  }

  Vector3 GetCurrentInput()
  {
    // applies forces from player input
    if (generator.isGrounded)
    {
      Vector3 mappedInput = MapToInputSpace(playerManager.inputManager.movementInput);
      if (playerManager.inputManager.runPressed)
      {
        // return playerSpeedMult * playerRunMult * MapToInputSpace(playerManager.inputManager.movementInput);
        Debug.DrawLine(transform.position, transform.position + playerSpeedMult * playerRunMult * Vector3.ProjectOnPlane(mappedInput, generator.limbSupportDirection), Color.cyan, Time.deltaTime);
        return playerSpeedMult * playerRunMult * Vector3.ProjectOnPlane(mappedInput, generator.limbSupportDirection).normalized;
      }
      else
      {
        // return playerSpeedMult * MapToInputSpace(playerManager.inputManager.movementInput);
        Debug.DrawLine(transform.position, transform.position + playerSpeedMult * Vector3.ProjectOnPlane(mappedInput, generator.limbSupportDirection), Color.cyan, Time.deltaTime);
        return playerSpeedMult * Vector3.ProjectOnPlane(mappedInput, generator.limbSupportDirection).normalized;
      }
    }
    else
    {
      // is no move when no ground
      return Vector3.zero;
    }
  }

  // void UpdateDistanceToGround()
  // {
  //   distanceToGround = maxGroundDistance;
  //   RaycastHit hit = new RaycastHit();
  //   if (Physics.Raycast (transform.position, -localUp, out hit, 1000f, layerMask)) {
  //     distanceToGround = Mathf.Min(hit.distance, maxGroundDistance);
  //   }
  // }

  public float upRotationSpeed = 0.02f;
  void UpdateTorques()
  {

    if(generator.isPlayer && !playerManager.inputManager.isSliding)
    {

      // set up local up
      if (generator.limbSupportDirection.sqrMagnitude != 0.0f)
      {
        Vector3 tempUp = generator.limbSupportDirection;
        localUp = Vector3.Lerp(localUp, tempUp, upRotationSpeed);
      }
      else if (localUp.sqrMagnitude == 0.0f) localUp = transform.up;

      if (!mouseRotationMode)
      {
        if (playerManager.inputManager.movementInput.sqrMagnitude != 0)
        {
          lastMoveInput = playerManager.inputManager.movementInput;
          lastMappedMoveLook = MapToInputSpace(playerManager.inputManager.movementInput);
          Quaternion moveLookRotation = Quaternion.LookRotation(lastMappedMoveLook, localUp);
          Quaternion lerpedLookRotation = Quaternion.Lerp(transform.rotation, moveLookRotation, rotationSpeed);
          desiredLookRotation = moveLookRotation;
        }
        else
        {
          lastMappedMoveLook = MapToInputSpace(lastMoveInput);
          Quaternion moveLookRotation = Quaternion.LookRotation(lastMappedMoveLook, localUp); // look in last direction if no inputs
          desiredLookRotation = moveLookRotation;
        }
      }
      else
      {
        Vector3 lastMappedMoveLook = MapToInputSpace(Vector3.forward);
        Quaternion moveLookRotation = Quaternion.LookRotation(lastMappedMoveLook, localUp);
        Quaternion lerpedLookRotation = Quaternion.Lerp(transform.rotation, moveLookRotation, rotationSpeed);
        desiredLookRotation = moveLookRotation;
      }

    }
    else if (generator.isPlayer && playerManager.inputManager.isSliding)
    {
      // again normals here
      Quaternion velocityLookRotation = Quaternion.LookRotation(rb.velocity, localUp);
      desiredLookRotation = Quaternion.Lerp(transform.rotation, velocityLookRotation, 2.0f*rotationSpeed);
    }

    // apply swing notion on top
    Vector3 swing = Vector3.zero;
    bool motionFlag = false;
    foreach(Limb lb in generator.equippedLimbs)
    {
      // only swords make a da jiggle.. boy subclasses are nifty
      if (lb is SwordArm || lb is GunArm)
      {
        if (lb.inMotion)
        {
          motionFlag = true;
          swing += lb.target.position - transform.position;
        }
      }
    }
    swing.Normalize();

    if (!generator.isPlayer && generator.playerManager.lookEnabled)
    {
      // if is not player, look at target if it's in range
      desiredLookRotation = Quaternion.LookRotation(generator.playerManager.target.position - transform.position, Vector3.up);
    }

    // if (motionFlag)
    if (false)
    {
      Quaternion swingLookRotation = Quaternion.LookRotation(Vector3.Reflect(swing, transform.right), localUp);
      desiredLookRotation = Quaternion.Lerp(desiredLookRotation, swingLookRotation, swingTurnMult*rotationSpeed);
    }

    // tilt body in direction of acceleration
    Quaternion desiredRotation = desiredLookRotation;
    // Quaternion desiredRotation = newRotation;

    float kp = (6f*torqueFrequency)*(6f*torqueFrequency)* 0.25f;
    float kd = 4.5f*torqueFrequency*torqueDamping;
    float dt = Time.fixedDeltaTime;
    float g = 1 / (1 + kd * dt + kp * dt * dt);
    float ksg = kp * g;
    float kdg = (kd + kp * dt) * g;
    Vector3 x;
    float xMag;
    Quaternion q = desiredRotation * Quaternion.Inverse(transform.rotation);

    // Q can be the-long-rotation-around-the-sphere eg. 350 degrees
    // We want the equivalant short rotation eg. -10 degrees
    // Check if rotation is greater than 190 degees == q.w is negative
    if (q.w < 0)
    {
      // Convert the quaterion to eqivalent "short way around" quaterion
      q.x = -q.x;
      q.y = -q.y;
      q.z = -q.z;
      q.w = -q.w;
    }
    q.ToAngleAxis (out xMag, out x);
    x.Normalize ();
    x *= Mathf.Deg2Rad;
    Vector3 pidv = kp * x * xMag - kd * rb.angularVelocity;
    Quaternion rotInertia2World = rb.inertiaTensorRotation * transform.rotation;
    pidv = Quaternion.Inverse(rotInertia2World) * pidv;
    pidv.Scale(rb.inertiaTensor);
    pidv = rotInertia2World * pidv;
    rb.AddTorque (pidv);

    // Calculate swing direction from frame to frame
    swingDirection = transform.forward - previousForward;
    previousForward = transform.forward;

  }

  void UpdateGravity()
  {

    // need a slight restoring force downwards to reach a standing position rather than only pushing up?
    if(generator.isGrounded) // have some support from limbs
    {
      float kp = (6f*ctrlFrequency)*(6f*ctrlFrequency)* 0.25f;
      float kd = 4.5f*ctrlFrequency*ctrlDamping;
      float dt = Time.fixedDeltaTime;
      float g = 1 / (1 + kd * dt + kp * dt * dt);
      float ksg = kp * g;
      float kdg = (kd + kp * dt) * g;
      Vector3 Pdes = Vector3.zero;
      if (generator.limbSupportAnchor.sqrMagnitude != 0.0f)
      {
        Pdes = generator.limbSupportAnchor;
      }
      else
      {
        Pdes = rb.position; // not touching anything?
      }

      if (generator.isPlayer)
      {
        // move the destination if we're a player
        Pdes += GetCurrentInput();
        Pdes += jumpBias;
      }
      Vector3 Vdes = Vector3.zero;
      Vector3 Pt0 = rb.position;
      Vector3 Vt0 = rb.velocity;
      Vector3 F = (Pdes - Pt0) * ksg + (Vdes - Vt0) * kdg;
      // need to apply force ONLY in localUp direction not side ta side and not down
      // dont apply if not in same dir as up
      // no WAY that comes to bite my ass
      Vector3 Fp = Vector3.Project(F, transform.up);
      rb.AddForce(Vector3.Lerp(Fp, F, restoringForce));

    }

    // update sliding mechanic
    if(playerManager.inputManager.isSliding)
    {
      rb.drag = 0.0f;
    }
    else
    {
      rb.drag = rbDrag;
    }
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    if (initalized)
    {
      UpdateTorques();
      UpdateGravity();
    }
    if (generator.isPlayer)
    {
      HandleJumpMotion();
      if (playerManager.inputManager.aiming)
      {
        mouseRotationMode = true;
      }
      else
      {
        mouseRotationMode = false;
      }
    }

    previousVelocity = rb.velocity;
    previousPosition = rb.position;

  }

  void HandleJumpMotion()
  {
    // update counts for delays
    if (jumpDelayCounter < jumpDelayCount && playerManager.inputManager.jumpPressed)
    {
      jumpDelayCounter += 1;
      jumpBias += (-jumpCrouchScaler*generator.limbSupportDirection);
    }
    else if (!playerManager.inputManager.jumpPressed && jumpDelayCounter > 0 && generator.isGrounded)
    {
      // we ride
      jumpDelayCounter -= 1;
      jumpBias += jumpHeightScaler*transform.up;

    }
    else if (!generator.isGrounded)
    {
      jumpBias = Vector3.zero;
      jumpDelayCounter = 0;
    }
  }

  public Vector3 MapToInputSpace(Vector3 worldInput)
  {
    Vector3 desiredVelocity;
    if (generator.cameraTransform) {

      Vector3 forward = generator.cameraTransform.forward - localUp * Vector3.Dot(generator.cameraTransform.forward, localUp);
      Vector3 right = Vector3.Cross(forward.normalized, localUp).normalized;
      desiredVelocity = (-worldInput.x*right + worldInput.z*forward);
    }
    else
    {
      desiredVelocity = worldInput;
    }
    return desiredVelocity;
  }

}
