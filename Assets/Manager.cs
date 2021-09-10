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
  public GameNetworkManager netManager;
  public int[] cachedEquippedLimbs;
  public Limb[] limbs; // THE list tm


  public static bool CheckEquality<T>(T[] first, T[] second)
  {
    return first.SequenceEqual(second);
  }

  public virtual void Initialize()
  {
    inputManager = GetComponent<InputManager>();
    creatureGenerator = GetComponent<CreatureGenerator>();
    particleContainer = GetComponent<ParticleContainer>();
    int[] tempLimbIds = creatureGenerator.RandomizeCreature();
    cachedEquippedLimbs = tempLimbIds;
    if (isLocalPlayer) CmdSyncLimbs(tempLimbIds);
    initialized = true;

    // get network manager to swap scenes
    netManager = FindObjectOfType<GameNetworkManager>();
    for(int i = 0; i < limbs.Length; i++)
    {
      netManager.spawnPrefabs.Add(creatureGenerator.limbs[i].gameObject);
    }
  }

  [ClientRpc]
  private void ClientUpdateLimbs(int[] limbIds)
  {
    // update cached limbs and update limbs
    if (!hasAuthority)
    {
      // update lims
      cachedEquippedLimbs = limbIds;
      print("remote player updating limbs..");
      creatureGenerator.SetLimbIds(limbIds);
    }
  }

  [Command]
  public void CmdSyncLimbs(int[] limbIds)
  {
    print("cmd syncing limb ids");
    ClientUpdateLimbs(limbIds);
  }

  [Command(requiresAuthority = false)]
  public void CmdDeleteLimb(GameObject limbToDelete)
  {
    // delete spawned limb when somebody else picks it up
    if (!limbToDelete) return;
    print($"command deleting: {limbToDelete.name}");
    NetworkServer.Destroy(limbToDelete);

  }
  [Command]
  public void CmdSpawnLimb(int spawnId)
  {
    // spawn limb so it shows up on all clients
    // put world pos
    print($"command spawning: {spawnId}");
    // GameObject limbCopy = Instantiate(limbs[spawnId].gameObject);
    Limb bit = Instantiate(limbs[spawnId], creatureGenerator.transform).GetComponent<Limb>();
    bit.Initialize(creatureGenerator, id, spawnId);
    NetworkServer.Spawn(limbCopy);
  }

  // non command versions
  public void SpawnLimb(int spawnId)
  {
    if (isLocalPlayer)
    {
      print($"limb w net id spawning: {spawnId}");
      CmdSpawnLimb(spawnId);
    }
    // if (isLocalPlayer) CmdSpawnLimb(null);
  }
  public void DeleteLimb(GameObject obj)
  {
    if (isLocalPlayer) DeleteLimb(obj);
  }

}
