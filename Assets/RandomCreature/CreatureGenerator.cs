using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Mirror;
using UnityEngine.VFX;

public class CreatureGenerator : MonoBehaviour
{

  // limb libraries
  public Body[] bodies;
  public Limb[] limbs;
  public Head[] heads;
  public LayerMask layerMask;
  public Transform cameraTransform;
  public bool isPlayer = false; // makes camera follow and inputs work
  public bool isGrounded = true; // depends on limbs and such...
  public bool built = false; // whether body is built yet
  public float limbSupport = 0.0f;
  public Vector3 limbSupportDirection = Vector3.zero;
  public Vector3 limbSupportAnchor = Vector3.zero;

  [HideInInspector]
  public List<AttachPoint> currentAttachPoints;
  [HideInInspector]
  public List<Vector3> attachPointOffsets;
  [HideInInspector]
  public enum LimbType {leg, arm, head}; // more to follow
  [HideInInspector]
  public Rigidbody rb;

  public int[] equippedLimbIds;
  public List<Head> equippedHeads = new List<Head>();
  public List<Limb> equippedLimbs = new List<Limb>();
  public List<Limb> nearbyLimbs = new List<Limb>();
  [HideInInspector]
  public OrbitCamera orbitCam;
  [HideInInspector]
  public Limb targetedLimb; // for picking up limbs
  [HideInInspector]
  public Body equippedBody;
  [HideInInspector]
  public MassController ctrl; // actual motion controller
  [HideInInspector]
  public Health health;
  [HideInInspector]
  public Energy energy;
  [HideInInspector]
  public Manager playerManager;

  private GameObject limbObj;
  private Vector3[] textOffsets;
  private RectTransform[] textTransforms;
  private Text[] limbTexts;
  private List<BoneCollider> limbColliders = new List<BoneCollider>();
  private CapsuleCollider bodyCollider;

  public int[] RandomizeCreature()
  {
    // build from ids but w random ids
    int[] tempLimbIds = new int[currentAttachPoints.Count];
    // equippedLimbIds = new int[currentAttachPoints.Count];
    for (int i = 0; i < currentAttachPoints.Count; i++) {
      tempLimbIds[i] = Random.Range(0, limbs.Length);
      // print($"new limb id rolled: {tempLimbIds[i]}");
    }
    return BuildFromIds(tempLimbIds);
  }

  public int[] BuildFromIds(int[] limbIds)
  {
    // do initial set up

    if (isPlayer)
    {
      orbitCam = cameraTransform.gameObject.GetComponent<OrbitCamera>();
      playerManager = gameObject.GetComponent<PlayerManager>();
    }
    else
    {
      // do equivalant enemy manager! has particles/inputs/ so on
      playerManager = gameObject.GetComponent<EnemyManager>();
    }

    health = gameObject.AddComponent<Health>();
    health.Initialize(this, (playerManager as PlayerManager).healthBar);

    energy = gameObject.AddComponent<Energy>();
    energy.Initialize(this, (playerManager as PlayerManager).energyBar);

    // build from scratch
    equippedBody = AddBody(transform);
    GetAttachPoints(equippedBody);
    ConfigurePhysics(equippedBody);

    equippedLimbIds = limbIds;
    SetLimbIds(equippedLimbIds);
    playerManager.refreshLimbs = true;

    GetLimbColliders(); // get independent bone colliders
    built = true;
    return equippedLimbIds;
  }

  public void SetLimbIds(int[] newIds)
  {
    // equippedLimbIds = newIds;
    // make limbs list match what ids are set
    for (int i=0; i < newIds.Length; i++)
    {
      if(newIds[i] < 0)
      {
        print($"NOT adding pt {i} idx {newIds[i]}");
        if (CheckLimbIndex(i))
        {
          Limb ghostLimb = GetLimbAtIndex(i);
          print($"removing for limb id : {i}");
          // playerManager.DeleteLimb(ghostLimb);
          // equippedLimbs[exId] = bit;
          equippedLimbs.Remove(ghostLimb);
          ghostLimb.Uninstall();
          Destroy(ghostLimb);
        }
      }
      else if (newIds[i] != equippedLimbIds[i])
      {
        print($"adding pt {i} idx {newIds[i]}");
        AddLimb(currentAttachPoints[i], i, newIds[i]);
      }
      else if (!CheckLimbIndex(i) && newIds[i] >= 0)
      {
        print($"adding at empty pt {i} idx {newIds[i]}");
        AddLimb(currentAttachPoints[i], i, newIds[i]);
      }
    }
    equippedLimbIds = newIds;
  }

  public void GetLimbColliders()
  {
    foreach(Limb lb in equippedLimbs)
    {
      limbColliders.AddRange(lb.boneColliders);
    }
  }

  public Body AddBody(Transform trs)
  {
    Body bod = Instantiate(bodies.PickOne(), trs).GetComponent<Body>();
    bod.Initialize(this);
    return bod;
  }

  public void AddLimb(AttachPoint pt, int id, int limbId)
  {
    // make room
    // check if pt has limb already

    if (CheckLimbIndex(id))
    {
      Limb ghostLimb = GetLimbAtIndex(id);
      print($"removing for limb id : {id}");
      // playerManager.DeleteLimb(ghostLimb);
      // equippedLimbs[exId] = bit;
      equippedLimbs.Remove(ghostLimb);
      ghostLimb.Uninstall();
      Destroy(ghostLimb);
    }
    Limb bit = Instantiate(limbs[limbId], pt.transform).GetComponent<Limb>();
    bit.Initialize(this, id, limbId);
    // spawn cross network
    equippedLimbs.Add(bit);

    // call sync operation

    // put correct id in array
    equippedLimbIds[id] = limbId;
    playerManager.refreshLimbs = true;
  }

  public void AddRandomLimb(AttachPoint pt, int id)
  {
    Limb[] ptLimbs = pt.GetLimbTypes();
    if (pt.restrictLimbs)
    {
      var restrictedLimbs = ptLimbs.Intersect(limbs);
      if (!restrictedLimbs.IsEmpty())
      {
        // while have not found good index
        bool indexFlag = false;
        int tempIndex = 0;
        int trueIndex = 0;
        while(!indexFlag)
        {
          tempIndex = Random.Range(0, limbs.Length);
          if (restrictedLimbs.Contains(limbs[tempIndex]))
          {
            indexFlag = true;
            trueIndex = tempIndex;
          }
        }
        Limb bit = Instantiate(limbs[trueIndex], pt.transform).GetComponent<Limb>();
        bit.Initialize(this, id, trueIndex);
        equippedLimbs.Add(bit);
        equippedLimbIds[trueIndex] = trueIndex;
      }
    }
    else
    {
      int ind = Random.Range(0, limbs.Length);
      Limb bit = Instantiate(limbs[ind], pt.transform).GetComponent<Limb>();
      bit.Initialize(this, id, ind);
      equippedLimbs.Add(bit);
      equippedLimbIds[id] = ind;
      // return bit;
    }
    // HACK land
    playerManager.refreshLimbs = true;
  }

  public void PickUpLimb(AttachPoint pt, Limb limbToPickUp, int id)
  {
    Limb[] ptLimbs = pt.GetLimbTypes(); // TODO make picking up limbs subject to point contraints
    if (nearbyLimbs.Contains(limbToPickUp)) nearbyLimbs.Remove(limbToPickUp);

    // track limb ids
    equippedLimbIds[id] = limbToPickUp.index;
    // do client call yaself
    SetLimbIds(equippedLimbIds);
    playerManager.CmdSyncLimbs(equippedLimbIds);
    playerManager.CmdDeleteLimb(limbToPickUp.sceneIndex);
    // delete picked up limb locally
    limbToPickUp.Uninstall();
  }

  public void RemoveLimb(int limbIdToRemove)
  {
    // does not copy limb and drop it, just drops limb at given index and removes from list
    bool limbRemovedFlag = false;
    List<Limb> newLimbList = new List<Limb>(equippedLimbs);
    foreach(Limb lb in equippedLimbs)
    {
      if(lb.id == limbIdToRemove)
      {
        lb.transform.parent = null;
        lb.RedgeDollToggle(true);
        if (lb.boneColliders.Count > 0)
        {
          lb.AddLimbForce(500.0f*lb.boneColliders[0].transform.forward, lb.boneColliders[0]);
        }
        lb.initialized = false;

        limbRemovedFlag = true;
        newLimbList.Remove(lb);
        // spawn on network
        // playerManager.netManager.spawnPrefabs.Add(lb.gameObject);
        playerManager.CmdSpawnLimb(lb.index);

        // assign scene index if not
        lb.sceneIndex = playerManager.sceneIndexCounter;
        equippedLimbIds[limbIdToRemove] = -1;
        playerManager.refreshLimbs = true;

        if (playerManager.isClientOnly)
        {
          // remove limb from worl
          lb.Uninstall();
        }
      }
    }

    if (limbRemovedFlag)
    {
      equippedLimbs = newLimbList;
      // also call for sync operation
      // playerManager.SendLimbIds(GetLimbIds());
    }
  }

  public bool CheckLimbIndex(int limbIndexCheck)
  {
    // dead simple, check for id in equippedLimbs
    foreach(Limb lb in equippedLimbs)
    {
      if(lb.id == limbIndexCheck)
      {
        return true;
      }
    }
    return false;
  }

  public Limb GetLimbAtIndex(int limbIndex)
  {
    // dead simple, check for id in equippedLimbs
    foreach(Limb lb in equippedLimbs)
    {
      if(lb.id == limbIndex)
      {
        return lb;
      }
    }
    return null;
  }

  public int GetLimbIndex(int limbId)
  {
    // dead simple, returns list index of id
    int idx = 0;
    foreach(Limb lb in equippedLimbs)
    {
      if(lb.id == limbId)
      {
        return idx;
      }
      idx += 1;
    }
    return -1;
  }

  public void ConfigurePhysics(Body bod)
  {
    // add rigidbody to CREATURE object
    rb = gameObject.AddComponent<Rigidbody>();
    rb.mass = bod.motionMass;
    rb.drag = bod.motionDrag;
    rb.useGravity = false;
    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    // add physics controller to creature
    ctrl = gameObject.AddComponent<MassController>();
    ctrl.Initialize(this, rb);

    // add body's collider
    bodyCollider = bod.GetBodyCollider();
    CapsuleCollider newCollider = gameObject.AddComponent<CapsuleCollider>();

    newCollider.center = bodyCollider.center;
    newCollider.radius = bodyCollider.radius;
    newCollider.height = bodyCollider.height;
    newCollider.material = bodyCollider.material;
    newCollider.direction = bodyCollider.direction;
    bodyCollider.enabled = false;

    // make limb attach points track generator
    limbObj = bod.GetLimbIdleTransform().gameObject;
    limbObj.transform.SetParent(transform);
    limbObj.transform.position = transform.position;
    limbObj.transform.rotation = transform.rotation;

    if (isPlayer)
    {
      //make camera follow body instead of creature?
      OrbitCamera cam = cameraTransform.gameObject.GetComponent<OrbitCamera>();
      cam.focus = bod.bodyPart;
    }

  }

  public void GetAttachPoints(Body bod)
  {
    // add attach point transforms to creature object
    AttachPoint[] bodyAttachPoints = bod.gameObject.GetComponentsInChildren<AttachPoint>();
    for (int i = 0; i < bodyAttachPoints.Length; i++) {
      bodyAttachPoints[i].transform.SetParent(bod.limbIdleTransform);
      currentAttachPoints.Add(bodyAttachPoints[i]);

      attachPointOffsets.Add(bodyAttachPoints[i].transform.position - bod.transform.position);
    }

  }

  public bool CheckStepping()
  {
    foreach(Limb lb in equippedLimbs)
    {
      if (lb is Leg)
      {
        if (lb.inMotion) return true;
      }
    }
    return false;
  }

  public int grappleCallCounter = 0;
  public int GrappleTurnAlternator()
  {
    // grappleCallCounter++;
    // must be betta. like click once to attach arm, click again to establish another arm, keep clicking to release from arm least recently attached... ya feel

    int maximumId = -1;
    int oldestGrappleId = -1;
    float carbonDate = Mathf.Infinity;
    foreach(Limb lb in equippedLimbs)
    {
      if (lb is GrappleArm)
      {
        GrappleArm lbGrapple = lb as GrappleArm;
        // if ANY grappler is not on cool down we dont go
        if (lbGrapple.shotOnCooldown) return -1;
        if (lbGrapple.grappledAt < carbonDate)
        {
          carbonDate = lbGrapple.grappledAt;
          oldestGrappleId = lb.id;
        }
        // if yaint grappled
        if (lbGrapple.grappledAt <= -1.0f)
        {
          if (lbGrapple.id > maximumId)
          {
            maximumId = lbGrapple.id;
          }
        }
      }
    }
    if (maximumId == -1)
    {
      // no limb not grappled
      return oldestGrappleId;
    }
    return maximumId;
  }

  public bool CheckPunching()
  {

    bool flag = false;
    for (int i=0; i < equippedLimbs.Count; i++)
    {
      if (equippedLimbs[i] is Arm)
      {
        if (equippedLimbs[i].inMotion)
        {
          flag = true;
        }
      }
    }
    return flag;
  }

  public Arm GetRandomArm()
  {
    // int armCounter = 0;
    List<Arm> armList = new List<Arm>();
    foreach (Limb lb in equippedLimbs)
    {
      if (lb is Arm)
      {
        armList.Add((Arm)lb);
      }
    }
    return armList.PickOne();
  }

  public void FixedUpdate()
  {

    if (built)
    {
      HandleLimbs();
      if (isPlayer)
      {
        HandleInventory();
      }
    }

  }

  public void HandleLimbs()
  {
    // does a few things, keeps limbs attached to body, checks support stuff, finds closest available limb to activate

    // update body attach points
    isGrounded = false;
    limbSupport = 0.0f;
    limbSupportDirection = Vector3.zero;
    limbSupportAnchor = Vector3.zero;
    // fuck all that just figure out what point to hit, centroid bb

    float totalConsumption = 0.0f;
    int groundedLimbs = 0;
    for (int i=0; i < equippedLimbs.Count; i++)
    {
      // equippedLimbs
      equippedLimbs[i].transform.GetChild(0).transform.position = equippedBody.transform.position + equippedBody.transform.TransformVector(attachPointOffsets[equippedLimbs[i].id]);
      limbSupport += equippedLimbs[i].pos.support;
      if (equippedLimbs[i].pos.isGrounded)
      {
        limbSupportAnchor += equippedLimbs[i].target.position + equippedLimbs[i].supportHeight*equippedLimbs[i].pos.groundNormal;
        limbSupportDirection += equippedLimbs[i].pos.groundNormal;
        groundedLimbs += 1;
      }

      // check if equippedLimbIds still match
      if (equippedLimbIds[i] != equippedLimbs[i].index)
      {
        playerManager.refreshLimbs = true;
      }

      // update energy consumption per limb
      totalConsumption += equippedLimbs[i].energyConsumption;

    }
    Debug.DrawLine(transform.position, transform.position + limbSupportDirection*2f, Color.red, Time.deltaTime);
    limbSupportDirection = (limbSupportDirection/groundedLimbs).normalized;
    limbSupportAnchor = limbSupportAnchor/groundedLimbs;

    // subtract consumption this frame from the total energy
    energy.Consume(totalConsumption);

    if (limbSupport != 0.0f)
    {
      isGrounded = true; // if at least one limb is touching the ground then we're 'supported'
    }
    // match limb idle targets to generator
    limbObj.transform.position = equippedBody.transform.position - equippedBody.idlePositionOffset;
    limbObj.transform.rotation = equippedBody.transform.rotation;

    foreach(BoneCollider bc in limbColliders)
    {
      if(bc.isHit)
      {
        bc.ClearHit();
      }
    }

    float minDistance = 10.0f; // larger than max dist limbs will add themselves
    bool targetedLimbFlag = false;
    foreach(Limb lb in nearbyLimbs)
    {
      float dist = Vector3.Distance(transform.position, transform.GetChild(0).transform.position);
      if (dist <= minDistance)
      {
        minDistance = dist;
        targetedLimb = lb;
        targetedLimbFlag = true;
      }
    }
    if (!targetedLimbFlag)
    {
      targetedLimb = null; // get rid of ref
    }
  }

  public void HandleInventory()
  {
    // do something special when the inventory button is pressed
    // orbitCam = cameraTransform.gameObject.GetComponent<OrbitCamera>();
    float oldCameraDist = orbitCam.distance;
    Vector2 oldCameraOrbitAngles = orbitCam.orbitAngles;

    // do inventory if's
    if (playerManager.inputManager.inventoryPressed && !playerManager.inputManager.inventoryOpen
    && !playerManager.inputManager.aimPressed && !playerManager.inputManager.aiming)
    {
      orbitCam.distance = 0.5f*oldCameraDist;
      playerManager.inputManager.inventoryOpen = true;
    }

    // inv 0 button
    else if (playerManager.inputManager.inventory0Pressed && playerManager.inputManager.inventoryOpen)
    {
      if (CheckLimbIndex(0)) // if have limb in slot, id is slot
      {
        RemoveLimb(0);
      }
    }
    // inv 1 button
    else if (playerManager.inputManager.inventory1Pressed && playerManager.inputManager.inventoryOpen)
    {
      if (CheckLimbIndex(1)) // if have limb in slot, id is slot
      {
        RemoveLimb(1);
      }
    }
    // inv 2 button
    else if (playerManager.inputManager.inventory2Pressed && playerManager.inputManager.inventoryOpen)
    {
      if (CheckLimbIndex(2)) // if have limb in slot, id is slot
      {
        RemoveLimb(2);
      }
    }
    // inv 3 button
    else if (playerManager.inputManager.inventory3Pressed && playerManager.inputManager.inventoryOpen)
    {
      if (CheckLimbIndex(3)) // if have limb in slot, id is slot
      {
        RemoveLimb(3);
      }
    }

    else if (targetedLimb != null)
    {
      if (playerManager.inputManager.inventory0Pressed && !playerManager.inputManager.inventoryOpen && targetedLimb.generatorNearFlag)
      {
        if (!CheckLimbIndex(0))
        {
          PickUpLimb(currentAttachPoints[0], targetedLimb, 0);
        }
      }
      else if (playerManager.inputManager.inventory1Pressed && !playerManager.inputManager.inventoryOpen && targetedLimb.generatorNearFlag)
      {
        if (!CheckLimbIndex(1))
        {
          PickUpLimb(currentAttachPoints[1], targetedLimb, 1);
        }
      }
      else if (playerManager.inputManager.inventory2Pressed && !playerManager.inputManager.inventoryOpen && targetedLimb.generatorNearFlag)
      {
        if (!CheckLimbIndex(2))
        {
          PickUpLimb(currentAttachPoints[2], targetedLimb, 2);
        }
      }
      else if (playerManager.inputManager.inventory3Pressed && !playerManager.inputManager.inventoryOpen && targetedLimb.generatorNearFlag)
      {
        if (!CheckLimbIndex(3))
        {
          PickUpLimb(currentAttachPoints[3], targetedLimb, 3);
        }
      }
    }
    if (!playerManager.inputManager.inventoryPressed && playerManager.inputManager.inventoryOpen)
    {
      orbitCam.distance = 2.0f*oldCameraDist;
      playerManager.inputManager.inventoryOpen = false;
    }
  }

  public void Die(bool particlesFlag = true)
  {
    // move back to origin and respawn
    // instead of origin, we should be rolling back to last safe position
    // also drop all interactions, grappled, grabs yada
    playerManager.inputManager.shootPressed = false;

    health.currentHealth = 10f;

    foreach(Limb lb in equippedLimbs)
    {
      if (lb is GrappleArm)
      {
        (lb as GrappleArm).Release();
      }
    }
    transform.position = Vector3.zero;
    rb.velocity = Vector3.zero; // respawn at zero speed pls


    StartCoroutine(Respawn());
  }

  // likely this should be in the player manager..
  public IEnumerator Respawn()
  {

    float blinkDuration = 1f;
    bool blinkToggle = false;
    float blinkStart = Time.time;

    while(Time.time < blinkStart + blinkDuration)
    {
      // blunk
      if (blinkToggle)
      {
        equippedBody.gameObject.SetActive(true);
        limbObj.gameObject.SetActive(true);
        blinkToggle = false;
      }
      else
      {
        equippedBody.gameObject.SetActive(false);
        limbObj.gameObject.SetActive(false);
        blinkToggle = true;
      }
      yield return null;
    }
    equippedBody.gameObject.SetActive(true);
    limbObj.gameObject.SetActive(true);
    foreach(Limb lb in equippedLimbs)
    {
      lb.traj.done = true; //
    }
  }

  public VisualEffect bodyParticles;
  public float impactSizeScaler = 1f;
  public float impactDamageScaler = 1f;
  void OnCollisionEnter(Collision collision)
  {
    // play particles w params based on impact speed and position
    if(layerMask == (layerMask | 1 << collision.gameObject.layer))
    {
      if (bodyParticles.HasFloat("impactSize"))
      {
        bodyParticles.SetFloat("impactSize", collision.impulse.magnitude*impactSizeScaler);
      }
      bodyParticles.Play();
      bodyParticles.gameObject.transform.position = collision.GetContact(0).point;

      // take damage
      if (health != null)
      {
        health.Damage(collision.impulse.magnitude*impactDamageScaler);
      }
    }
  }

  public void OnCollisionStay(Collision collision)
  {
    // play particles w params based on impact speed and position
    if(layerMask == (layerMask | 1 << collision.gameObject.layer))
    {
      // activate goosh for duration of particles and while moving
      bodyParticles.gameObject.transform.position = collision.GetContact(0).point;
    }
  }

  public void OnCollisionExit(Collision other)
  {
    // play particles w params based on impact speed and position
    if(layerMask == (layerMask | 1 << other.gameObject.layer))
    {
      bodyParticles.Stop();
      bodyParticles.gameObject.transform.localPosition = Vector3.zero;
    }
  }

  public void OnDrawGizmos()
  {
    if (isGrounded)
    {
      Gizmos.DrawIcon(limbSupportAnchor, "Light Gizmo.tiff", false);
    }
  }

}
