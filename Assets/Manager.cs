using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using System.Linq;

public class Manager : NetworkBehaviour
{
  [Header("testing gui")]
  public bool refreshLimbs = false;

  [Header("Manager")]
  public ParticleContainer particleContainer;
  public InputManager inputManager;
  public CreatureGenerator creatureGenerator;
  public Transform aimTarget;
  public NavMeshAgent agent;
  public Transform target;
  public bool lookEnabled = false;
  public bool initialized = false;
  [Header("Sync Vars")] // appropriate
  // [SyncVar]
  public int sceneIndexCounter = 0; // will be same as one on net manager

  public GameNetworkManager netManager;
  public int[] cachedEquippedLimbs;
  public Limb[] limbs; // THE list tm

  // public Dictionary<System.Guid, int> limbGuidDict = new Dictionary<System.Guid, int>();


  public static bool CheckEquality<T>(T[] first, T[] second)
  {
    return first.SequenceEqual(second);
  }

  public virtual void Initialize()
  {
    inputManager = GetComponent<InputManager>();
    creatureGenerator = GetComponent<CreatureGenerator>();
    particleContainer = GetComponent<ParticleContainer>();
    // int[] tempLimbIds = creatureGenerator.BuildFromIds(presetLimbIds);
    int[] tempLimbIds = creatureGenerator.RandomizeCreature();
    cachedEquippedLimbs = tempLimbIds;
    if (isLocalPlayer) CmdSyncLimbs(tempLimbIds);
    initialized = true;

    // get network manager to swap scenes
    netManager = FindObjectOfType<GameNetworkManager>();
    for(int i = 0; i < creatureGenerator.limbs.Length; i++)
    {
      netManager.spawnPrefabs.Add(creatureGenerator.limbs[i].gameObject);
      // System.Guid limbAssetId = System.Guid.NewGuid();
      // System.Guid limbAssetId = creatureGenerator.limbs[i].gameObject.GetComponent<NetworkIdentity>().assetId;
      // limbGuidDict.Add(limbAssetId, i);
      // NetworkClient.RegisterSpawnHandler(creatureGenerator.limbs[i].gameObject.GetComponent<NetworkIdentity>().assetId, spawn, unspawn);
    }
  }

  [ClientRpc]
  private void ClientSyncLimbs(int[] limbIds)
  {
    // update cached limbs and update limbs
    // update lims
    cachedEquippedLimbs = limbIds;
    // print("remote player updating limbs..");
    creatureGenerator.SetLimbIds(limbIds);
    // if (!hasAuthority)
    // {
    // }
  }

  [Command]
  public void CmdSyncLimbs(int[] limbIds)
  {
    // print("cmd syncing limb ids");
    ClientSyncLimbs(limbIds);
  }

  [Command(requiresAuthority = false)]
  public void CmdDeleteLimb(int limbSceneIndex)
  {
    print($"command deleting: {limbSceneIndex}");
    ClientDeleteLimb(limbSceneIndex);

  }

  [Command(requiresAuthority = false)]
  public void CmdSpawnLimb(int spawnId)
  {
    // spawn limb so it shows up on all clients
    // put world pos
    print($"command spawning: {spawnId}");
    // ask clients to make limb
    // sceneIndexCounter += 1;
    if (netManager == null) netManager = FindObjectOfType<GameNetworkManager>();
    netManager.sceneIndexCounter += 1;
    sceneIndexCounter = netManager.sceneIndexCounter;
    ClientSpawnLimb(spawnId, sceneIndexCounter);
    // ClientIncrementSceneIndex();
  }

  [Command(requiresAuthority = false)]
  public void CmdIncrementSceneIndex()
  {
    ClientIncrementSceneIndex();
  }

  [ClientRpc]
  private void ClientIncrementSceneIndex()
  {
    sceneIndexCounter += 1;
  }

  [ClientRpc]
  private void ClientDeleteLimb(int sceneIndex)
  {
    // delete spawned limb when somebody else picks it up
    Limb[] foundLimbs = FindObjectsOfType<Limb>();
    foreach(Limb lb in foundLimbs)
    {
      if (lb.sceneIndex == sceneIndex)
      {
        // remoob
        print($"client found limb: {sceneIndex} and is deleting");
        lb.Uninstall();
        return;
      }
    }
  }

  [ClientRpc]
  private void ClientSpawnLimb(int spawnId, int sceneId)
  {
    // if (!isLocalPlayer) return;
    print("client spawning limb");
    sceneIndexCounter = sceneId;
    Limb bit = Instantiate(creatureGenerator.limbs[spawnId], creatureGenerator.transform).GetComponent<Limb>();
    // NetworkServer.Spawn(bit.gameObject);

    bit.sceneIndex = sceneIndexCounter;

    bit.Initialize(creatureGenerator, spawnId, spawnId);
    // drop limb now?
    bit.transform.parent = null;
    bit.RedgeDollToggle(true);
    if (bit.boneColliders.Count > 0)
    {
     bit.AddLimbForce(500.0f*bit.boneColliders[0].transform.forward, bit.boneColliders[0]);
    }

    // turn on network transform
    // bit.gameObject.GetComponent<NetworkTransform>().enabled = true;
    bit.initialized = false;

  }

  // public GameObject LimbSpawn(Vector3 position, System.Guid assetId)
  // {
  //   // spawn..?
  //   print("limb spawn delegate called!");
  //   int spawnId = limbGuidDict[assetId];
  //   GameObject bitObj = Instantiate(creatureGenerator.limbs[spawnId].gameObject, position, new Quaternion());
  //
  //   Limb bit = bitObj.GetComponent<Limb>();
  //   bit.Initialize(creatureGenerator, spawnId, spawnId);
  //   // drop limb now?
  //   bit.transform.parent = null;
  //   bit.RedgeDollToggle(true);
  //   if (bit.boneColliders.Count > 0)
  //   {
  //    bit.AddLimbForce(500.0f*bit.boneColliders[0].transform.forward, bit.boneColliders[0]);
  //   }
  //   bit.initialized = false;
  //   return bitObj;
  // }
  //
  // public void LimbUnSpawn(GameObject spawned)
  // {
  //   // remove?
  //   Debug.Log("limb unspawn delegate called!");
  //   Destroy(spawned);
  // }

  // [ClientRpc]
  // public void ClientInitNewLimb(GameObject bitObj, int spawnId)
  // {
  //   if (hasAuthority)
  //   {
  //     if (bitObj == null)
  //     {
  //       print("limb is dead long live the limb");
  //       return;
  //     }
  //     Limb bit = bitObj.GetComponent<Limb>();
  //     bit.Initialize(creatureGenerator, spawnId, spawnId);
  //     // drop limb now?
  //     bit.transform.parent = null;
  //     bit.RedgeDollToggle(true);
  //     if (bit.boneColliders.Count > 0)
  //     {
  //       bit.AddLimbForce(500.0f*bit.boneColliders[0].transform.forward, bit.boneColliders[0]);
  //     }
  //     bit.initialized = false;
  //   }
  // }

  // non command versions
  // public void SpawnLimb(int spawnId)
  // {
  //   print($"limb w net id spawning: {spawnId}");
  //   if (isLocalPlayer)
  //   {
  //     CmdSpawnLimb(spawnId, );
  //   }
  //   // if (isLocalPlayer) CmdSpawnLimb(null);
  // }
  // public void DeleteLimb(GameObject obj)
  // {
  //   if (isLocalPlayer) DeleteLimb(obj);
  // }

}
