using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Limb : MonoBehaviour
{
  [HideInInspector]
  public struct LimbPosition
  {
    public Vector3 worldPosition;
    public Quaternion worldRotation;
    public bool isGrounded;
    public Vector3 groundNormal;
    public float support;
  }

  [Header("Limb Motion")]

  public Transform bone;
  public Transform pole;
  public Transform idleTarget;
  public InfoText infoText;
  public float stepRadius;
  public float limbLength;
  public float supportHeight;
  public float support = 1.0f;
  public float supportScaler = 1;
  public float supportAngle = 20f; // the maximum angle difference between player up and ground normal a limb will provide support at
  public float velocityPositionScaler = 0.2f; // amount that velocity influences limb position
  public float twitchScale = 0.25f;
  public float twitchRandomScale = 0.1f;
  public int pointNumber = 100;
  public LayerMask layerMask; // interaction layer mask
  [Range(0,1)]
  public float nervousness = 0.05f;
  public float idleMoveScale = 0.05f;
  public float positionSmoothTime = 0.01f;
  public float rotationSmoothSpeed = 0.01f;
  public float smoothTargetDistanceScale = 5f;
  public float energyConsumption = 0.0f;
  public float supportEnergyScaler = 0.01f;
  public List<BoneCollider> boneColliders;


  [Header("Debug Fields")]

  public bool generatorNearFlag = false;
  public bool playerNearFlag = false;
  public bool drawRaycasts = false;
  public bool initialized = false;
  public int id;
  public int sceneIndex = -1; // limbs index in the scene, unique synched over net

  [Header("Damage Params")]
  public float damage;

  // hidden parameters
  [HideInInspector]
  public Transform target;
  [HideInInspector]
  public Transform smoothTarget;
  [HideInInspector]
  public Transform player;
  [HideInInspector]
  public LimbPosition pos;
  [HideInInspector]
  public CreatureGenerator generator;
  [HideInInspector]
  public bool inMotion;
  [HideInInspector]
  public Vector3 previousPosition = Vector3.zero;
  [HideInInspector]
  public Vector3 idlePositionOffset = Vector3.zero;
  [HideInInspector]
  public Manager playerManager;
  [HideInInspector]
  public Trajectory traj;
  [HideInInspector]
  public int index; // limbs index in the generator limb list

  private FastIKFabric ik;
  private Vector3 smoothRefVelocity = Vector3.zero;
  private CreatureGenerator[] creatureGenerators; // all creature creatureGenerators in scene
  private Vector3 currentDirectionalInput = Vector3.zero;
  private CharacterInputs inputs;
  private GameObject textObject;

  public virtual void Initialize(CreatureGenerator gen, int id, int limbIndex)
  {
    // id number
    this.id = id;
    // set starting positions
    this.player = gen.transform;
    // move to generator's layer
    this.gameObject.layer = gen.gameObject.layer;

    // add trajectory handler
    traj = gameObject.AddComponent<Trajectory>();

    // add ik script to end bone
    this.ik = bone.parent.gameObject.AddComponent<FastIKFabric>();
    this.ik.ChainLength = 2; // bones then hand
    this.target = ik.Target; // makes it's own target
    this.target.gameObject.name = gameObject.name + "_" + id.ToString() + "_" + "target";
    this.ik.Pole = pole;
    this.ik.Delta = 0.001f;

    this.target.position = idleTarget.position;
    this.generator = gen;
    this.layerMask = gen.layerMask; // copy this over
    this.inMotion = false;

    // get order of self in limb list, should not change in game
    this.index = limbIndex;
    // for (int i = 0; i < gen.limbs.Length; i++)
    // {
    //   if (gen.limbs[i] is this)
    //   {
    //     this.index = i; // our index in limb list
    //     print($"limb got index {index}");
    //     break;
    //   }
    // }

    foreach(BoneCollider boneCol in boneColliders)
    {
      boneCol.Initialize(layerMask);
    }

    this.smoothTarget = new GameObject().transform;
    this.smoothTarget.parent = gameObject.transform;
    this.smoothTarget.position = target.position;
    this.ik.Target = smoothTarget;
    this.playerManager = gen.playerManager;

    // initialize information text per limb
    this.infoText = Instantiate(infoText, transform.position, new Quaternion());
    this.infoText.transform.SetParent(transform); // saves text from being deleted and makes nice hierarchy
    this.infoText.Initialize(bone, generator, id);

    RedgeDollToggle(false);

    // mark targets and text to not be deleted on load
    DontDestroyOnLoad(target);


    this.initialized = true;
  }

  public virtual void UpdateSupport()
  {

    // inefficient but conservative
    if (supportScaler > 0f)
    {
      // also saves the new position for kicks
      pos = FindGround();
      support = pos.support;
      generator.energy.Consume(support*supportEnergyScaler);
    }
    else
    {
      support = 0f;
    }

  }

  public virtual void Uninstall()
  {
    if (textObject != null) Destroy(textObject);
    if (infoText.target.gameObject != null) Destroy(infoText.target.gameObject);
    if (target.gameObject != null) Destroy(target.gameObject);
    if (gameObject != null) Destroy(gameObject);
  }

  public virtual List<BoneCollider> GetBoneColliders()
  {
    return boneColliders;
  }

  public virtual void RedgeDollToggle(bool redge)
  {
    // turn off ik system
    ik.enabled = !redge;
    initialized = !redge;
    // print("setting limb initialized value to " + !redge);
    foreach(BoneCollider boneCol in boneColliders)
    {
      boneCol.RedgeDollToggle(redge);
    }

  }

  // adds force to the specified bone, might do offset n stuff later
  public virtual void AddLimbForce(Vector3 force, BoneCollider boneCollider)
  {
    if (!initialized) // in redge mode
    {
      boneCollider.rb.AddForce(force);
    }
    // AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
  }

  public virtual Vector3 AirPosition()
  {
    // Vector3 vel = transform.position + previousPosition;
    // Vector3 horizontalSpeed = new Vector3(vel.x, 0.0f, vel.z);
    // going up so tilt towards velocity vec
    return idleTarget.position;
  }

  public virtual void FrameUpdate()
  {

    if (initialized)
    {

      UpdateSupport();

      if (drawRaycasts)
      {
        Debug.DrawLine(smoothTarget.position, previousPosition, Color.white, 1.5f);
      }

      if (Random.value <= nervousness)
      {
        idlePositionOffset = idleMoveScale*GetRandomSpherical();
      }

      if (generator.isPlayer)
      {
        if (playerManager.inputManager.inventoryPressed)
        {
          infoText.TextEnable(true);
        }
        else
        {
          infoText.TextEnable(false);
        }
      }
      else
      {
        infoText.TextEnable(false);
      }

      // move towards target with smooth damp based on current speed
      float dist = smoothTargetDistanceScale*Vector3.Distance(smoothTarget.position, target.position);
      Vector3 newPosition = Vector3.SmoothDamp(smoothTarget.position, target.position, ref smoothRefVelocity, positionSmoothTime/dist);
      smoothTarget.position = newPosition;
      // match bones rotation smoothly too
      smoothTarget.rotation = Quaternion.Lerp(smoothTarget.rotation, target.rotation, rotationSmoothSpeed);
      // bone.rotation = smoothTarget.rotation;

      previousPosition = smoothTarget.position;
    }
    else
    {
      // look for players coming near to ask for pick up
      creatureGenerators = FindObjectsOfType<CreatureGenerator>();
      foreach(CreatureGenerator gen in creatureGenerators)
      {
        float dist = Vector3.Distance(gen.transform.position, transform.GetChild(0).transform.position);
        if(dist <= 2.0f) // only want to pick one thank you (this is a todo btw)
        {
          generatorNearFlag = true;
          // add self to nearbylimbs list
          if (!gen.nearbyLimbs.Contains(this)) gen.nearbyLimbs.Add(this);
          if(gen.isPlayer)
          {
            playerNearFlag = true;
          }
          infoText.TextEnable(true);
        }
        else
        {
          infoText.TextEnable(false);
          if (gen.nearbyLimbs.Contains(this)) gen.nearbyLimbs.Remove(this);
        }
      }
    }
  }

  public virtual void Twitch(Vector3 twitchDir, float twitchMagnitude, float randomScale)
  {
    // jerk target a random amount
    float xRand = 2.0f*Random.value - 1.0f;
    float yRand = 2.0f*Random.value - 1.0f;
    Vector3 leftOfTwitch = Vector3.Cross(twitchDir, Vector3.up).normalized;
    Vector3 downOfTwitch = Vector3.Cross(twitchDir, Vector3.left).normalized;

    target.position += twitchDir*twitchMagnitude + xRand*randomScale*leftOfTwitch + yRand*randomScale*downOfTwitch;
  }

  public virtual bool LimbGrounded()
  {
    // checks if bone's collider is overlapping w something
    Collider col = bone.gameObject.GetComponent<Collider>();
    Collider[] overlappedCols = Physics.OverlapBox(col.bounds.center, col.bounds.extents, bone.rotation, generator.layerMask);
    if (overlappedCols.Length > 0)
    {
      // bone in current position is hitting something
      // print("overlap method ground detection");
      return true;
    }
    return false;
  }

  public static Vector3 GetRandomSpherical()
  {
    float phi = 2*Mathf.PI*Random.value;
    float theta = 2*Mathf.PI*Random.value;
    float r = Random.value;
    Vector3 spherePosition = new Vector3(r*Mathf.Cos(phi)*Mathf.Sin(theta), r*Mathf.Sin(phi)*Mathf.Sin(theta), r*Mathf.Cos(theta));
    return spherePosition;
  }

  public Vector3[] PointsOnSphere(int n)
  {
    List<Vector3> upts = new List<Vector3>();
    float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
    float off = 2.0f / n;
    float x = 0;
    float y = 0;
    float z = 0;
    float r = 0;
    float phi = 0;

    for (var k = 0; k < n; k++){
      y = k * off - 1 + (off /2);
      r = Mathf.Sqrt(1 - y * y);
      phi = k * inc;
      x = Mathf.Cos(phi) * r;
      z = Mathf.Sin(phi) * r;

      upts.Add(new Vector3(x, y, z));
    }
    Vector3[] pts = upts.ToArray();
    return pts;
  }

  public virtual LimbPosition FindGround()
  {
    LimbPosition outputPosition = new LimbPosition();
    Vector3 groundNormal = player.up;

    // -----------------------------
    bool groundFlag = false;
    Vector3[] spherePts = PointsOnSphere(pointNumber);

    // also add some bias to center of raycast ball, that way we can account for body speed in preferred locations
    Vector3 bias = generator.ctrl.previousVelocity*velocityPositionScaler;
    // Vector3 bias = Vector3.zero;

    Vector3 currentGroundPosition = Vector3.zero;
    float currentMinDistance = Mathf.Infinity;

    // this might be directional
    foreach (Vector3 pointOnSphere in spherePts) // only all of em
    {
      if (drawRaycasts) Debug.DrawLine(transform.position + bias, transform.position + bias + limbLength*pointOnSphere, Color.green,Time.deltaTime);
      RaycastHit hitPoint = new RaycastHit(); // efficiency bro
      if (Physics.Raycast(transform.position + bias, pointOnSphere, out hitPoint, limbLength, layerMask))
      {
        if (drawRaycasts) Debug.DrawLine(transform.position + bias, transform.position + bias + pointOnSphere*hitPoint.distance - 0.05f*Vector3.right, Color.white,Time.deltaTime);
        if (hitPoint.distance < currentMinDistance && Vector3.Angle(player.up, -pointOnSphere) < supportAngle)
        {
          // one last check to make sure we're not inside an obstacle
          if (!Physics.Raycast(transform.position + bias, -pointOnSphere, out RaycastHit heet, limbLength, layerMask))
          {
            // print("inside hit!!");
            groundFlag = true; // found ground
            currentGroundPosition = transform.position + bias + pointOnSphere*hitPoint.distance;
            groundNormal = hitPoint.normal.normalized; // average normals
            currentMinDistance = hitPoint.distance;
          }
        }
      }
    }
    // -----------------------------
    // and we breath
    if (groundFlag && (currentGroundPosition - transform.position).sqrMagnitude < limbLength*limbLength && generator.energy.hasGas)
    {
      // print($"found position that is grounded {currentMinDistance} units away");// found ground
      outputPosition.worldPosition = currentGroundPosition;
      outputPosition.isGrounded = true;
      outputPosition.groundNormal = groundNormal;
      outputPosition.support = supportScaler*(supportHeight - currentMinDistance);
      return outputPosition;
    }
    // print("found position that is NOT grounded");
    outputPosition.worldPosition = AirPosition();
    outputPosition.isGrounded = false;
    outputPosition.groundNormal = groundNormal; // no normal in air!
    outputPosition.support = 0; // no support neither

    return outputPosition;
  }


}
