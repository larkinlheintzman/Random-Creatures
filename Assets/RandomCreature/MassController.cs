using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MassController : MonoBehaviour
{

  bool initalized = false;

  public Rigidbody rb;
  CharacterInputs input;
  public Transform playerInputSpace;
  public CreatureGenerator generator;
  public PlayerManager playerManager;

  public Vector3 currentPosition;
  public Vector3 currentVelocity;
  public Vector3 jumpVelocity;
  public Vector3 previousVelocity = Vector3.zero;
  public Vector3 previousPosition = Vector3.zero;
  public Vector3 swingDirection = Vector3.zero;
  public Vector3 previousForward = Vector3.zero;
  public Quaternion desiredLookRotation = Quaternion.Euler(0f,0f,0f);
  public float accelerationTilt = 100.0f;


  public bool rotationMode = false;

  public float standingHeight = 2.0f;
  public float rbDrag;
  public float rbMass;

  public float ctrlFrequency = 3f;
  public float ctrlDamping = 1f;
  public float torqueFrequency = 3f;
  public float torqueDamping = 1f;
  public float swingTurnMult = 1f;
  private float groundSpeed;
  private float maxGroundDistance = 20.0f;
  private float distanceToGround = 0.0f;
  private Vector3 lastMappedMoveLook = Vector3.zero;

  // bools to keep track
  bool isJumping = false;
  float jumpDelayCount = 20.0f; // maximum possible jump charge
  float jumpDelayBase = 5.0f; // base jump delay
  float jumpDelayCounter = 0.0f; // counter for delay

  [HideInInspector, Range(1,20)]
  public float playerSpeedMult = 4.0f;
  [HideInInspector, Range(1,10)]
  public float playerAirSpeedMult = 1.0f;
  [HideInInspector, Range(1,10)]
  public float playerRunMult = 1.5f;
  [HideInInspector, Range(0,1)]
  public float playerRotationSpeed = 0.1f;
  [HideInInspector, Range(1,20)]
  public float playerJumpSpeed = 20.0f;
  [HideInInspector, Range(0,5)]
  public float groundedMaxDistance = 1.05f; // set more better
  [HideInInspector, Range(0,1)]
  public float rotationSpeed = 0.2f;

  [HideInInspector]
  private int layerMask;

  private void Awake()
  {
    lastMappedMoveLook = transform.forward;
  }

  public void Initialize(Transform inputSpace, CreatureGenerator gen, Rigidbody rigidbod)
  {

    // copy all es shit
    rb = rigidbod;
    rbDrag = rb.drag;
    rbMass = rb.mass;
    rb.useGravity = true;
    rb.isKinematic = false;

    generator = gen;
    layerMask = gen.layerMask;

    playerInputSpace = inputSpace;
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
        rb.AddForce(playerSpeedMult * playerRunMult * generator.MapToInputSpace(playerManager.inputManager.movementInput));
      }
      else
      {
        rb.AddForce(playerSpeedMult * generator.MapToInputSpace(playerManager.inputManager.movementInput));
      }
    }
    else {
      rb.AddForce(playerSpeedMult * playerAirSpeedMult * generator.MapToInputSpace(playerManager.inputManager.movementInput));
    }
  }

  void UpdateDistanceToGround()
  {
    distanceToGround = maxGroundDistance;
    RaycastHit hit = new RaycastHit();
    if (Physics.Raycast (transform.position, -Vector3.up, out hit, Mathf.Infinity, layerMask)) {
      distanceToGround = Mathf.Min(hit.distance, maxGroundDistance);
    }
  }

  void UpdateTorques()
  {

    if(generator.isPlayer && !playerManager.inputManager.isSliding)
    {

      if (!rotationMode)
      {
        if (playerManager.inputManager.movementInput.sqrMagnitude != 0)
        {
          lastMappedMoveLook = generator.MapToInputSpace(playerManager.inputManager.movementInput);
          Quaternion moveLookRotation = Quaternion.LookRotation(lastMappedMoveLook, Vector3.up);
          Quaternion lerpedLookRotation = Quaternion.Lerp(transform.rotation, moveLookRotation, rotationSpeed);
          desiredLookRotation = moveLookRotation;
          // transform.rotation = lerpedLookRotation;
        }
        else
        {
          // TODO put normals here instead of ups
          Quaternion moveLookRotation = Quaternion.LookRotation(lastMappedMoveLook, Vector3.up); // look in last direction if no inputs
          desiredLookRotation = moveLookRotation;
        }
      }
      else
      {
        Vector3 lastMappedMoveLook = generator.MapToInputSpace(Vector3.forward);
        Quaternion moveLookRotation = Quaternion.LookRotation(lastMappedMoveLook, transform.up);
        Quaternion lerpedLookRotation = Quaternion.Lerp(transform.rotation, moveLookRotation, rotationSpeed);
        desiredLookRotation = moveLookRotation;
        // transform.rotation = lerpedLookRotation;
      }

    }
    else if (generator.isPlayer && playerManager.inputManager.isSliding)
    {
      // again normals here
      Quaternion velocityLookRotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
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
    if (generator.isPlayer && motionFlag)
    {
      Quaternion swingLookRotation = Quaternion.LookRotation(Vector3.Reflect(swing, transform.right), Vector3.up);
      desiredLookRotation = Quaternion.Lerp(transform.rotation, swingLookRotation, swingTurnMult*rotationSpeed);
    }

    if (!generator.isPlayer)
    {
      // needs brains
      desiredLookRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
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
    rb.AddForce(9.81f*Vector3.down);

    if(generator.isGrounded) // have some support from limbs
    {
      float kp = (6f*ctrlFrequency)*(6f*ctrlFrequency)* 0.25f;
      float kd = 4.5f*ctrlFrequency*ctrlDamping;
      float dt = Time.fixedDeltaTime;
      float g = 1 / (1 + kd * dt + kp * dt * dt);
      float ksg = kp * g;
      float kdg = (kd + kp * dt) * g;
      Vector3 Pdes = rb.position + Vector3.up*(generator.limbSupport - standingHeight);
      // Vector3 Pdes = rb.position + Vector3.up*standingHeight;
      Vector3 Vdes = Vector3.zero;
      Vector3 Pt0 = rb.position;
      Vector3 Vt0 = rb.velocity;
      Vector3 F = (Pdes - Pt0) * ksg + (Vdes - Vt0) * kdg;
      // obviously Vector3.up/down will be changed to match ground angle at some point
      // rb.AddForce(9.81f*Vector3.up);
      if (F.y >= 0.0f)
      {
        rb.AddForce(new Vector3(0.0f, F.y, 0.0f));
      }
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
        rotationMode = true;
      }
      else
      {
        rotationMode = false;
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

      jumpVelocity = new Vector3(0.0f, (((jumpDelayCount - jumpDelayCounter) / (jumpDelayCount)) * playerJumpSpeed), 0.0f);
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

    if (playerManager.inputManager.jumpPressed && generator.isGrounded && !isJumping)
    {
      // Handle jump motion
      isJumping = true;
      jumpDelayCounter = jumpDelayCount;
      // print("jump requested");
    }
  }

}
