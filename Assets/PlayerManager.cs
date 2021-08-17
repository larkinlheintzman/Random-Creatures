using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleContainer)),RequireComponent(typeof(InputManager))]
public class PlayerManager : NetworkBehaviour
{

  // #region Singleton

  public Transform player;
  public Camera playerCamera;
  public ParticleContainer particleContainer;
  public InputManager inputManager;
  public NetworkTransformChild networkTransform;
  public bool initialized = false;

  public void Initialize() // start here because awake is too fast
  {

    networkTransform = GetComponent<NetworkTransformChild>();
    networkTransform.clientAuthority = true; // ??

    Debug.Log("local player joining: " + isLocalPlayer.ToString());
    if (isLocalPlayer)
    {
      this.inputManager = GetComponent<InputManager>();
      this.inputManager.Initialize();
      OrbitCamera orbitCamera = this.playerCamera.gameObject.GetComponent<OrbitCamera>();
      orbitCamera.Initialize();
      this.particleContainer = GetComponent<ParticleContainer>();
      // turn off lobby camera and turn on own
      GameObject.Find("LobbyCamera").SetActive(false);
      CreatureGenerator creatureGenerator = GetComponentInChildren<CreatureGenerator>(); // should only be 1
      creatureGenerator.RandomizeCreature(); // make pler
    }
    else
    {
      // turn off player manager and camera/listener
      this.inputManager = GetComponent<InputManager>();
      this.inputManager.isLocalPlayer = false;
      this.inputManager.Initialize();
      this.particleContainer = GetComponent<ParticleContainer>();

      CreatureGenerator creatureGenerator = GetComponentInChildren<CreatureGenerator>(); // should only be 1
      creatureGenerator.RandomizeCreature(); // make pler

      this.playerCamera.gameObject.SetActive(false); // lazy. need to point client texts are localplayer camera
    }
    // NetworkIdentity netId = player.gameObject.GetComponent<NetworkIdentity>();
    // netId.AssignClientAuthority(conn);
    initialized = true;
  }

  public override void OnStartClient()
  {
    Debug.Log("starting up player obj!");
    // NetworkIdentity clientId = conn.identity;
    // PlayerManager playerManager = clientId.gameObject.GetComponent<PlayerManager>();
    Initialize();
  }

  // #endregion

}
