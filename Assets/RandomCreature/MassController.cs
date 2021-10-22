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
  public Quaternion desiredLookRotation = Quaternion.Euler(0f,0f,0f);
  public float accelerationTilt = 100.0f;

  public bool mouseRotationMode = false;

  public float standingHeight = 2.0f;
  public float rbDrag;
  public float rbMass;

  public float ctrlFrequency = 1f;
  public float ctrlDamping = 1f;
  public float torqueFrequency = 3f;
  public float torqueDamping = 1f;
  public float swingTurnMult = 1f;
  [Range(0f,1f)]
  public float restoringForce = 0.1f;
  private float groundSpeed;
  private float maxGroundDistance = 20.0f;
  private float distanceToGround = 0.0f;
  private Vector3 lastMappedMoveLook = Vector3.zero;

  // bools to keep track
  bool isJumping = false;
  float jumpDelayCount = 20.0f; // maximum possible jump charge
  float jumpDelayBase = 5.0f; // base jump delay
  float jumpDelayCounter = 0.0f; // counter for delay

  public float playerSpeedMult = 20.0f;
  public float playerAirSpeedMult = 1.0f;
  public float playerRunMult = 1.5f;
  public float playerRotationSpeed = 0.1f;
  public float playerJumpSpeed = 20.0f;
  public float groundedMaxDistance = 1.05f; // set more better
  public float rotationSpeed = 0.2f;

  [HideInInspector]
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

    isJumping = false;
    initalized = true;
    playerManager = gen.playerManager;
  }

  void ApplyCurrentInput()
  {
    // applies forces from player input
    if (generator.isGrounded)
    {
      if (playerManager.inputManager.runPressed)
      {
        rb.AddForce(playerSpeedMult * playerRunMult * MapToInputSpace(playerManager.inputManager.movementInput));
      }
      else
      {
        rb.AddForce(playerSpeedMult * MapToInputSpace(playerManager.inputManager.movementInput));
      }
    }
    else {
      rb.AddForce(playerSpeedMult * playerAirSpeedMult * MapToInputSpace(playerManager.inputManager.movementInput));
    }
  }

  void UpdateDistanceToGround()
  {
    distanceToGround = maxGroundDistance;
    RaycastHit hit = new RaycastHit();
    if (Physics.Raycast (transform.position, -localUp, out hit, 1000f, layerMask)) {
      distanceToGround = Mathf.Min(hit.distance, maxGroundDistance);
    }
  }

  void UpdateTorques()
  {

    if(generator.isPlayer && !playerManager.inputManager.isSliding)
    {

      if (!mouseRotationMode)
      {
        if (playerManager.inputManager.movementInput.sqrMagnitude != 0)
        {
          lastMappedMoveLook = MapToInputSpace(playerManager.inputManager.movementInput);
          Quaternion moveLookRotation = Quaternion.LookRotation(lastMappedMoveLook, localUp);
          Quaternion lerpedLookRotation = Quaternion.Lerp(transform.rotation, moveLookRotation, rotationSpeed);
          desiredLookRotation = moveLookRotation;
        }
        else
        {
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
      // Vector3 Pdes = rb.position + localUp*(generator.limbSupport - standingHeight);
      Vector3 Pdes = Vector3.zero;
      if (generator.limbSupportAnchor.sqrMagnitude != 0.0f)
      {
        Pdes = rb.position + generator.limbSupportDirection*(standingHeight);
      }
      else
      {
        Pdes = rb.position + generator.limbSupportDirection*(generator.limbSupport - standingHeight); // not touching anything?
        // Pdes = rb.position;
      }
      Vector3 Vdes = Vector3.zero;
      Vector3 Pt0 = rb.position;
      Vector3 Vt0 = rb.velocity;
      Vector3 F = (Pdes - Pt0) * ksg + (Vdes - Vt0) * kdg;
      // need to apply force ONLY in localUp direction not side ta side and not down
      // dont apply if not in same dir as up
      // no WAY that comes to bite my ass
      if (Vector3.Dot(F, transform.up) > 0f)
      {
        Vector3 Fp = Vector3.Project(F, transform.up);
        rb.AddForce(Vector3.Lerp(Fp, F, restoringForce));
      }

    }
    else
    {

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
      UpdateDistanceToGround();
      UpdateGravity();
      UpdateTorques();
    }
    if (generator.isPlayer)
    {
      ApplyCurrentInput();
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
    if (isJumping && jumpDelayCounter > jumpDelayBase)
    {

      // jumpVelocity = new Vector3(0.0f, (((jumpDelayCount - jumpDelayCounter) / (jumpDelayCount)) * playerJumpSpeed), 0.0f);
      jumpVelocity = localUp*(((jumpDelayCount - jumpDelayCounter) / (jumpDelayCount)) * playerJumpSpeed);
      if (playerManager.inputManager.jumpPressed) // still holding button
      {
        jumpDelayCounter -= 1; // count down to lift off
      }
      else // let go of button
      {
        jumpDelayCounter = jumpDelayBase; // skip to end of charge phase
      }

    }
    else if (isJumping)// in base delay
    {
      jumpDelayCounter -= 1;

      if (jumpDelayCounter == 0 && generator.isGrounded)
      {

        rb.velocity += jumpVelocity;
        // print("jump fired: " + jumpVelocity);
        isJumping = false; // we have jumped

      }
      else if (jumpDelayCounter <= 0)
      {
        jumpDelayCounter = 0;
        isJumping = false; // we could not jump
      }
    }

    if (playerManager.inputManager.jumpPressed && generator.isGrounded && !isJumping && generator.energy.hasGas)
    {
      // Handle jump motion
      isJumping = true;
      jumpDelayCounter = jumpDelayCount;
      // print("jump requested");
    }
  }

  public Vector3 MapToInputSpace(Vector3 worldInput)
  {
    Vector3 desiredVelocity;
    if (generator.cameraTransform) {

      Vector3 forward = generator.cameraTransform.forward - localUp * Vector3.Dot(generator.cameraTransform.forward, localUp);
      Vector3 right = Vector3.Cross(forward.normalized, localUp).normalized;
      // Vector3 right = generator.cameraTransform.right - localUp * Vector3.Dot(generator.cameraTransform.right, localUp);
      // Debug.DrawLine(transform.position, transform.position + 2f*right, Color.red, 0.1f);
      // Debug.DrawLine(transform.position, transform.position + 2f*forward, Color.blue, 0.1f);
      desiredVelocity = (-worldInput.x*right + worldInput.z*forward);
    }
    else
    {
      desiredVelocity = worldInput;
    }
    // Debug.DrawLine(transform.position, transform.position + 4f*worldInput, Color.green, 0.1f);
    // Debug.DrawLine(transform.position, transform.position + 5f*desiredVelocity, Color.white, 0.1f);
    return desiredVelocity;
  }


}
