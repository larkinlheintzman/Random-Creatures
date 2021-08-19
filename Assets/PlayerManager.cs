using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleContainer)),RequireComponent(typeof(InputManager))]
public class PlayerManager : NetworkBehaviour
{

  public Transform player;
  public Camera playerCamera;
  public ParticleContainer particleContainer;
  public InputManager inputManager;
  public CreatureGenerator creatureGenerator;
  public NetworkTransformChild networkTransform;
  public Camera lobbyCamera;
  public AudioListener lobbyAudio;
  public bool initialized = false;

  public void Initialize()
  {
    networkTransform = GetComponent<NetworkTransformChild>();
    networkTransform.clientAuthority = true; // ?

    GameObject lobbyCameraObject = GameObject.Find("LobbyCamera");
    lobbyCamera = lobbyCameraObject.GetComponent<Camera>();
    lobbyAudio = lobbyCameraObject.GetComponent<AudioListener>();

    Debug.Log("local player joining: " + isLocalPlayer.ToString());
    if (isLocalPlayer)
    {
      inputManager = GetComponent<InputManager>();
      inputManager.Initialize();

      OrbitCamera orbitCamera = playerCamera.gameObject.GetComponent<OrbitCamera>();
      orbitCamera.Initialize();
      particleContainer = GetComponent<ParticleContainer>();

      // turn off lobby camera and turn on own
      lobbyCamera.enabled = false;
      lobbyAudio.enabled = false;
      creatureGenerator = GetComponentInChildren<CreatureGenerator>(); // should only be 1
      creatureGenerator.RandomizeCreature(); // make pler
    }
    else
    {
      // turn off player manager and camera/listener
      inputManager = GetComponent<InputManager>();
      inputManager.isLocalPlayer = false;
      particleContainer = GetComponent<ParticleContainer>();
      creatureGenerator = GetComponentInChildren<CreatureGenerator>(); // should only be 1
      creatureGenerator.RandomizeCreature(); // make pler

      playerCamera.gameObject.SetActive(false); // lazy. need to point client texts are localplayer camera
    }

    initialized = true;
  }

  public void Update()
  {
      if(initialized && isLocalPlayer)
      {
        if (inputManager.inputsChangedFlag) {
          inputManager.inputsChangedFlag = false;
          Debug.Log("registered input change from player: " + netId.ToString());
          SendInputsToServer(inputManager.PackageInputs());
        }
        // SendInputsToServer(inputManager.PackageInputs());

      }
  }

  [Command]
  public void SendInputsToServer(bool[] playerInputs)
  {
    // send inputs to all clients
    SendInputsToClients(playerInputs);
  }

  [ClientRpc]
  private void SendInputsToClients(bool[] playerInputs)
  {
    // only those clients that aren't players
    if (!hasAuthority) inputManager.ReadInputs(playerInputs);
  }

  public override void OnStartClient()
  {
    Initialize();
  }

  public override void OnStopClient()
  {
    if (isLocalPlayer)
    {
      lobbyCamera.enabled = true;
      lobbyAudio.enabled = true;
      // turn off own camera
      playerCamera.gameObject.SetActive(false);
    }
    if (creatureGenerator != null) creatureGenerator.Die();
    base.OnStopClient();
  }

}
