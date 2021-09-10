using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

  [HideInInspector]
  public List<AttachPoint> currentAttachPoints;
  [HideInInspector]
  public List<Vector3> attachPointOffsets;
  [HideInInspector]
  public enum LimbType {leg, arm, head}; // more to follow
  [HideInInspector]
  public Rigidbody rb;
  public OrbitCamera orbitCam;

  public int[] equippedLimbIds;
  // [HideInInspector]
  public List<Head> equippedHeads = new List<Head>();
  // [HideInInspector]
  public List<Limb> equippedLimbs = new List<Limb>();
  // [HideInInspector]
  public List<Limb> nearbyLimbs = new List<Limb>();
  // [HideInInspector]
  public Limb targetedLimb; // for picking up limbs
  [HideInInspector]
  public Body equippedBody;
  [HideInInspector]
  // [HideInInspector]
  public MassController ctrl; // actual motion controller
  public Health health;

  public Manager playerManager;

  private GameObject idleLimbObj;
  private Vector3[] textOffsets;
  private RectTransform[] textTransforms;
  private Text[] limbTexts;
  private List<BoneCollider> limbColliders = new List<BoneCollider>();
  private CapsuleCollider bodyCollider;

  public int[] RandomizeCreature()
  {
    // do initial set up
    health = gameObject.AddComponent<Health>();
    health.Initialize(this);

    if (isPlayer)
    {
      orbitCam = cameraTransform.gameObject.GetComponent<OrbitCamera>();
      playerManager = transform.parent.gameObject.GetComponent<PlayerManager>();
    }
    else
    {
      // do equivalant enemy manager! has particles/inputs/ so on
      playerManager = gameObject.GetComponent<EnemyManager>();
    }


    // build from scratch
    equippedBody = AddBody(transform);
    GetAttachPoints(equippedBody);

    if (isPlayer) {
      ConfigurePlayerPhysics(equippedBody);
    } else {
      ConfigureEnemyPhysics(equippedBody);
    }

    // select limb indexes from pool
    int[] tempLimbIds = new int[currentAttachPoints.Count];
    equippedLimbIds = new int[currentAttachPoints.Count];
    for (int i = 0; i < currentAttachPoints.Count; i++) {
      tempLimbIds[i] = Random.Range(0, limbs.Length);
      print($"new limb id rolled: {tempLimbIds[i]}");
    }
    equippedLimbIds = tempLimbIds;
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

  public int[] GetLimbIds()
  {
    return equippedLimbIds;
    // // gets limbs[] indexs of each equippedLimb
    // int[] returnIds = new int[equippedLimbs.Count];
    // int counter = 0;
    // foreach(Limb lb in equippedLimbs)
    // {
    //   // returnIds[counter] = limbs.IndexOf(lb);
    //   int limbsIndex = 0;
    //   for (int i = 0; i<limbs.Length; i++)
    //   {
    //     if (limbs[i] == lb)
    //     {
    //       returnIds[counter] = limbsIndex;
    //       break;
    //     }
    //   }
    //   // returnIds[counter] = limbsIndex;
    //   counter += 1;
    // }
    // return returnIds;
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

    limbToPickUp.id = id;
    limbToPickUp.RedgeDollToggle(false);
    limbToPickUp.initialized = true;
    limbToPickUp.transform.parent = pt.transform;
    equippedLimbs.Add(limbToPickUp);
    if (nearbyLimbs.Contains(limbToPickUp)) nearbyLimbs.Remove(limbToPickUp);

    // set pos and rotation back
    limbToPickUp.transform.localScale = Vector3.one;
    limbToPickUp.transform.localPosition = Vector3.zero;
    limbToPickUp.transform.localEulerAngles = Vector3.zero;

    // track limb ids
    equippedLimbIds[id] = limbToPickUp.index;
    playerManager.refreshLimbs = true;
    // delete DAMGER
    // playerManager.DeleteLimb(limbToPickUp.gameObject);
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
        playerManager.SpawnLimb(lb.index);
      }
    }

    if (limbRemovedFlag)
    {
      equippedLimbs = newLimbList;
      equippedLimbIds[limbIdToRemove] = -1;
      playerManager.refreshLimbs = true;
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

  public void ConfigurePlayerPhysics(Body bod)
  {
    // add rigidbody to CREATURE object
    rb = gameObject.AddComponent<Rigidbody>();
    rb.mass = bod.motionMass;
    rb.drag = bod.motionDrag;
    rb.useGravity = true;

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
    bodyCollider.isTrigger = true;

    // make limb attach points track generator
    idleLimbObj = bod.GetLimbIdleTransform().gameObject;
    idleLimbObj.transform.SetParent(transform);
    idleLimbObj.transform.position = transform.position;
    idleLimbObj.transform.rotation = transform.rotation;

    //make camera follow body instead of creature?
    OrbitCamera cam = cameraTransform.gameObject.GetComponent<OrbitCamera>();
    cam.focus = bod.bodyPart;

  }

  public void ConfigureEnemyPhysics(Body bod)
  {
    // add rigidbody to CREATURE object
    rb = gameObject.AddComponent<Rigidbody>();
    rb.mass = bod.motionMass;
    rb.drag = bod.motionDrag;
    rb.useGravity = true;

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
    bodyCollider.isTrigger = true;

    // make limb attach points track generator
    idleLimbObj = bod.GetLimbIdleTransform().gameObject;
    idleLimbObj.transform.SetParent(transform);
    idleLimbObj.transform.position = transform.position;
    idleLimbObj.transform.rotation = transform.rotation;

    // //make camera follow body instead of creature?
    // OrbitCamera cam = cameraTransform.gameObject.GetComponent<OrbitCamera>();
    // cam.focus = bod.bodyPart;
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

  public bool CheckStepping(int id)
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
    for (int i=0; i < equippedLimbs.Count; i++)
    {
      // equippedLimbs
      equippedLimbs[i].transform.GetChild(0).transform.position = equippedBody.transform.position + equippedBody.transform.TransformVector(attachPointOffsets[equippedLimbs[i].id]);
      limbSupport += equippedLimbs[i].support;
    }

    if (limbSupport != 0.0f)
    {
      isGrounded = true; // if at least one limb is touching the ground then we're 'supported'
    }
    // match limb idle targets to generator
    idleLimbObj.transform.position = equippedBody.transform.position - equippedBody.idlePositionOffset;
    idleLimbObj.transform.rotation = equippedBody.transform.rotation;

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

  public Vector3 MapToInputSpace(Vector3 worldInput)
  {
    Vector3 desiredVelocity;
    if (cameraTransform) {
      Vector3 forward = cameraTransform.forward;
      forward.y = 0f;
      forward.Normalize();
      Vector3 right = cameraTransform.right;
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

  public void Die(bool particlesFlag = true)
  {
    if (particlesFlag && playerManager != null) playerManager.particleContainer.PlayParticle(4, transform.position);
    // make limbs remove themselves too
    foreach(Limb lb in equippedLimbs)
    {
      lb.Uninstall();
    }
    gameObject.SetActive(false);
    // to be respawned later
  }

}
