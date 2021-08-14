using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicsController : MonoBehaviour
{

  bool initalized = false;

  // [HideInInspector]
  public Rigidbody rb;
  CharacterInputs input;
  // LedgeDetection ledgeDectector;
  // [HideInInspector]
  public Transform playerInputSpace;
  public CreatureGenerator generator;

  [HideInInspector]
  public Vector3 currentMovement;
  [HideInInspector]
  public Vector3 currentPosition;
  [HideInInspector]
  public Vector3 currentVelocity;
  [HideInInspector]
  public Vector3 jumpVelocity;

  public bool rotationMode = false;

  bool isGrounded = true;
  private float groundSpeed;
  private float maxGroundDistance = 20.0f;
  private float distanceToGround = 0.0f;


  // button booleans
  bool movementPressed;
  bool runPressed = false;
  bool rollPressed = false;
  bool punchPressed = false;
  bool ragdollPressed = false;
  bool jumpPressed = false;

  // bools to keep track
  bool isJumping = false;
  bool isAirborne = false;
  bool isHanging = false;
  int jumpDelayCount = 20; // maximum possible jump charge
  int jumpDelayBase = 5; // base jump delay
  int jumpDelayCounter = 0; // counter for delay

  [HideInInspector, Range(1,20)]
  public float playerSpeedMult = 15.0f;
  [HideInInspector, Range(1,10)]
  public float playerAirSpeedMult = 4.0f;
  [HideInInspector, Range(1,10)]
  public float playerRunMult = 1.5f;
  [HideInInspector, Range(1,10)]
  public float playerMaxSpeed = 3.0f; // player ground speed max
  [HideInInspector, Range(1,10)]
  public float playerMaxAirSpeed = 8.0f; // player air speed max
  [HideInInspector, Range(0,1)]
  public float playerRotationSpeed = 0.1f;
  [HideInInspector, Range(1,20)]
  public float playerMinJumpSpeed = 1f;
  [HideInInspector, Range(1,20)]
  public float playerMaxJumpSpeed = 2f;
  [HideInInspector, Range(0,5)]
  public float groundedMaxDistance = 1.05f; // set more better
  [HideInInspector, Range(0,1)]
  public float rotationSpeed = 0.2f;
  // shooty params
  // public GameObject boolet;
  // public float launchForce;
  [HideInInspector]
  private int layerMask;

  private void Awake()
  {
    input = new CharacterInputs();
    // initalized = false;
  }

  public void Initialize(Transform inputSpace, CreatureGenerator gen, Rigidbody rigidbod)
  {

    // copy all es shit
    rb = rigidbod;
    generator = gen;
    layerMask = gen.layerMask;

    playerInputSpace = inputSpace;

    isAirborne = false;
    isHanging = false;
    isJumping = false;
    isGrounded = true;

    // input.controls.Shoot.performed += ctx => ShootBoolet(ctx.ReadValueAsButton());

    input.controls.ZAxis.performed += ctx =>
    {
      currentMovement.z = ctx.ReadValue<float>();
      currentMovement.Normalize();
      movementPressed = currentMovement.x != 0 || currentMovement.z != 0;

      // HandleWalkAnimation(movementPressed);
      // Debug.Log(currentMovement);
    };

    input.controls.XAxis.performed += ctx =>
    {
      currentMovement.x = ctx.ReadValue<float>();
      currentMovement.Normalize();
      movementPressed = currentMovement.x != 0 || currentMovement.z != 0;

      // HandleWalkAnimation(movementPressed);
      // Debug.Log(currentMovement);
    };

    input.controls.Run.performed += ctx =>
    {
      runPressed = ctx.ReadValueAsButton();

      // HandleWalkAnimation(movementPressed);
      // Debug.Log(runPressed);
    };

    input.controls.Roll.performed += ctx =>
    {
      rollPressed = ctx.ReadValueAsButton();
      // Debug.Log(runPressed);
      // can we just set animation bools here?
      // HandleRollAnimation(rollPressed);
    };

    input.controls.Punch.performed += ctx =>
    {
      punchPressed = ctx.ReadValueAsButton();
      // HandlePunchAnimation(punchPressed);
    };

    // jump action
    input.controls.Jump.performed += ctx =>
    {
      jumpPressed = ctx.ReadValueAsButton();
      // HandleJumpAnimation(jumpPressed);
    };

    // ragdoll action started
    input.controls.Ragdoll.started += ctx =>
    {
      ragdollPressed = ctx.ReadValueAsButton();
      // Debug.Log("button down, val : " + ragdollPressed);
      // HandleRagdollAnimation(ragdollPressed);
    };
    // ragdoll action started
    input.controls.Ragdoll.performed += ctx =>
    {
      ragdollPressed = ctx.ReadValueAsButton();
      if (!ragdollPressed)
      {
        // Debug.Log("button up, val : " + ragdollPressed);
        // HandleRagdollAnimation(ragdollPressed);
      }
    };

    initalized = true;
    Debug.Log("body configured");
  }

  public Vector3 MapToInputSpace(Vector3 worldInput)
  {
    Vector3 desiredVelocity;
    if (playerInputSpace) {
      Vector3 forward = playerInputSpace.forward;
      forward.y = 0f;
      forward.Normalize();
      Vector3 right = playerInputSpace.right;
      right.y = 0f;
      right.Normalize();
      desiredVelocity = (forward * worldInput.z + right * worldInput.x);
    }
    else
    {
      desiredVelocity = worldInput;
    }
    // Debug.Log("desired velocity mapped: " + desiredVelocity);
    return desiredVelocity;
  }

  void ApplyCurrentInput()
  {
    // Debug.Log("applying current input: " + currentMovement);
    groundSpeed = Mathf.Sqrt(rb.velocity.x*rb.velocity.x + rb.velocity.z*rb.velocity.z);
    // applies forces from player input
    if (!isAirborne)
    {
      if (runPressed)
      {
        rb.AddForce(playerSpeedMult * playerRunMult * MapToInputSpace(currentMovement));
        // rb.AddForce(rb.position + playerSpeedMult * playerRunMult * MapToInputSpace(currentMovement) * Time.fixedDeltaTime);
        NormalizeVelocity(playerMaxSpeed * playerRunMult);
      }
      else
      {
        rb.AddForce(playerSpeedMult * MapToInputSpace(currentMovement));
        // rb.AddForce(rb.position + playerSpeedMult * MapToInputSpace(currentMovement) * Time.fixedDeltaTime);
        NormalizeVelocity(playerMaxSpeed);
      }
    }
    else {

      rb.AddForce(playerAirSpeedMult * MapToInputSpace(currentMovement));
      // rb.MovePosition(rb.position + playerAirSpeedMult * MapToInputSpace(currentMovement) * Time.fixedDeltaTime);
      NormalizeVelocity(playerMaxAirSpeed);

    }
  }

  void NormalizeVelocity(float maxSpeed)
  {

    if (groundSpeed > (maxSpeed))
    {
      // normalize velocity in x and z directions
      currentVelocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
      currentVelocity.Normalize();
      rb.velocity = new Vector3(currentVelocity.x * maxSpeed, rb.velocity.y, currentVelocity.z * maxSpeed);
      // Debug.Log("Normalized velocity");
    }

  }

  private void UpdateRecoveryTimes()
  {
    UpdateGrounded();
    UpdateDistanceToGround();
    // UpdateHangPoint(); // TODO not ready yet

  }

  // probably useful at some point
  void triggerAnimation(string animationKey)
  {

  }

  void UpdateGrounded()
  {
    // cast rays from radius around player
    // isGrounded = Physics.Raycast(transform.position + 0.1f * Vector3.up, -Vector3.up, groundedMaxDistance + 0.1f);
    if (Physics.Raycast(transform.position + 0.1f * Vector3.up, -Vector3.up, groundedMaxDistance + 0.1f, layerMask) != isGrounded)
    {
      isGrounded = Physics.Raycast(transform.position + 0.1f * Vector3.up, -Vector3.up, groundedMaxDistance + 0.1f);
      Debug.Log("grounded set to: " + isGrounded);
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

  // void UpdateHangPoint()
  // {
  //   // isHanging = Physics.Raycast(ledgeDetectionTransform.position, MapToInputSpace(Vector3.forward), ledgeGrabDistance);
  //   if (isHanging != ledgeDectector.DetectLedge())
  //   {
  //     isHanging = ledgeDectector.DetectLedge();
  //     Debug.Log("hanging set to: " + isHanging);
  //   }
  // }

  // Update is called once per frame
  void FixedUpdate()
  {
    if(initalized && generator.isPlayer)
    {
      UpdateRecoveryTimes();
      // Handle animations
      HandleActions();
      HandleJumpMotion();
      HandleRotation();
    }
    else
    {
      // Debug.Log("not initalized");
    }


  }

  void HandleActions()
  {
    // priority sorted (kinda)

    if (jumpPressed && isGrounded && !isAirborne && !isJumping && !isHanging)
    {
      // Debug.Log("starting jump animation");
      // triggerAnimation("Jumping Up");

      // Handle jump motion
      isJumping = true;
      jumpDelayCounter = jumpDelayCount;
      Debug.Log("triggering jump at: " + Time.time);

      ApplyCurrentInput();

    }

    if (punchPressed && isGrounded && !isAirborne && !isJumping && !isHanging)
    {

      // Debug.Log("starting punch animation");
      // triggerAnimation("PunchLeft");
      // Debug.Log("setting lock for " + "punch" + " to " + animationLockCount);

    }

    // Handle walk animation
    if (movementPressed && !runPressed && isGrounded && !isAirborne && !isJumping && !isHanging)
    {
      // Debug.Log("starting walk animation");
      // triggerAnimation("Walking");

      // Handle walk motion
      ApplyCurrentInput();

    }

    if (movementPressed && runPressed && isGrounded && !isAirborne && !isJumping && !isHanging)
    {
      // Debug.Log("starting run animation");
      // triggerAnimation("Running");

      // Handle run motion
      ApplyCurrentInput();
    }

    //Handle idle animation
    if (!movementPressed && !jumpPressed && isGrounded && !isAirborne && !isJumping && !isHanging)
    {
      // Debug.Log("starting idle animation");
      // triggerAnimation("Idle");;
    }

    // Handle falling animation
    if (!isGrounded && !isHanging)
    {
      // ApplyCurrentInput();
      // triggerAnimation("Falling Idle");
      // Debug.Log("starting idle air animation");
      isAirborne = true;

    }

    // Handle landing animation
    if (isGrounded && isAirborne && rb.velocity.y <= 0.0f && !isHanging)
    {
      // ApplyCurrentInput();
      // triggerAnimation("Falling To Landing");
      // update animation speed based on vertical speed
      isAirborne = false;
      // Debug.Log("is landing, is airborne: " + isAirborne);
      // Debug.Log("is (still) landing, is jumping: " + isJumping);

      // Handle motion
      ApplyCurrentInput();

    }

    if (isHanging)
    {
      // Debug.Log("is hanging");
      rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
      rb.AddForce(Vector3.forward);

      // this would go in the catch animation anyway
      if (rb.useGravity)
      {
        rb.useGravity = false;
      }
      if (isGrounded && !rb.useGravity)
      {
        rb.useGravity = true;
      }

    }




    if (!isJumping && isAirborne)
    {
      // Debug.Log("moving in air");
      // Handle air motion
      ApplyCurrentInput();

    }


  }

  void HandleJumpMotion()
  {
    // update counts for delays
    if (isJumping && jumpDelayCounter > jumpDelayBase)
    {
      if (jumpPressed) // still holding button
      {

        jumpDelayCounter -= 1; // count down to lift off
        // float jumpBlendingValue = 1.0f - (jumpDelayCounter*1.0f)/jumpDelayCount;
        // animator.SetFloat("JumpBlend", jumpBlendingValue);

        jumpVelocity = new Vector3(0.0f,
        ((jumpDelayCount - jumpDelayCounter * 1.0f) / (jumpDelayCount * 1.0f)) * (playerMaxJumpSpeed - playerMinJumpSpeed) + playerMinJumpSpeed, 0.0f);
        // Debug.Log("jomp pressed with: " + jumpVelocity + " counter at: " + jumpDelayCounter);

      }
      else // let go of button
      {

        jumpVelocity = new Vector3(0.0f,
        ((jumpDelayCount - jumpDelayCounter * 1.0f) / (jumpDelayCount * 1.0f)) * (playerMaxJumpSpeed - playerMinJumpSpeed) + playerMinJumpSpeed, 0.0f);
        // Debug.Log("jomp not pressed with: " + jumpVelocity + " counter at: " + jumpDelayCounter);

        jumpDelayCounter = jumpDelayBase; // skip to end of charge phase

        // float jumpBlendingValue = 1.0f - (jumpDelayCounter*1.0f)/jumpDelayCount;
        // animator.SetFloat("JumpBlend", jumpBlendingValue);

      }
    }
    else if (isJumping)// in base delay
    {
      jumpDelayCounter -= 1;

      // float jumpBlendingValue = 1.0f - (jumpDelayCounter*1.0f)/jumpDelayCount;
      // animator.SetFloat("JumpBlend", jumpBlendingValue);

      if (jumpDelayCounter == 0 && isGrounded)
      {

        rb.velocity += 10.0f*jumpVelocity;
        // Debug.Log("jump fired at: " + Time.time);

        isJumping = false; // we have jumped

      }
      else if (jumpDelayCounter <= 0)
      {
        jumpDelayCounter = 0;
        isJumping = false; // we could not jump
      }
    }
  }

  void HandleRotation()
  {
    // currentPosition = transform.position;
    // currentVelocity.Normalize();
    if (!rotationMode)
    {
      if (currentMovement.sqrMagnitude != 0)
      {
        Vector3 mappedCurrentMovement = MapToInputSpace(currentMovement);
        Quaternion moveLookRotation = Quaternion.LookRotation(mappedCurrentMovement, transform.up);
        Quaternion lerpedLookRotation = Quaternion.Lerp(transform.rotation, moveLookRotation, rotationSpeed);
        transform.rotation = lerpedLookRotation;
      }
    }
    else
    {
      Vector3 mappedCurrentMovement = MapToInputSpace(Vector3.forward);
      Quaternion moveLookRotation = Quaternion.LookRotation(mappedCurrentMovement, transform.up);
      Quaternion lerpedLookRotation = Quaternion.Lerp(transform.rotation, moveLookRotation, rotationSpeed);
      transform.rotation = lerpedLookRotation;
    }


    // if (rotationEnabled)
    // {
    //   Vector3 camTarget = playerInputSpace.position - transform.position;
    //   // Debug.DrawLine(transform.position, transform.position - new Vector3(camTarget.x, 0.0f, camTarget.z), Color.green, 0.2f);
    //   transform.LookAt(transform.position - new Vector3(camTarget.x, 0.0f, camTarget.z));
    // }

  }


  void OnEnable ()
  {
      input.controls.Enable();
  }

  void OnDisable ()
  {
      input.controls.Disable();
  }

}
