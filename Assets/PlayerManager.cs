using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ParticleContainer)),RequireComponent(typeof(InputManager))]
public class PlayerManager : Manager
{

  public Transform player;
  public Camera playerCamera;
  public GameObject playerCameraObject;
  public NetworkTransformChild networkTransform;
  public Camera lobbyCamera;
  public GameObject lobbyCameraObject;
  public AudioListener lobbyAudio;

  public int numPlayers = 0;
  public bool allPlayersReadyFlag = false;
  public bool indPlayerReadyFlag = false;

  public void PlayerCameraActive(bool active)
  {
    // turns on the players camera and disables the lobby camera
    // Debug.Log($"turning on/off {active} player {gameObject.name}'s camera");
    if (lobbyCameraObject == null) lobbyCameraObject = GameObject.Find("LobbyCamera");
    if (lobbyCameraObject == null) return; // no lobby camera we yeet
    lobbyCamera = lobbyCameraObject.GetComponent<Camera>();
    lobbyAudio = lobbyCameraObject.GetComponent<AudioListener>();
    AudioListener playerAudio = playerCamera.gameObject.GetComponent<AudioListener>();

    if (active)
    {
      OrbitCamera orbitCamera = playerCamera.transform.parent.gameObject.GetComponent<OrbitCamera>();
      if (!orbitCamera.initialized)
      {
        orbitCamera.Initialize();
      }
      playerCamera.enabled = true;
      playerAudio.enabled = true;
      playerCameraObject.SetActive(true);
      lobbyCamera.enabled = false;
      lobbyAudio.enabled = false;
    }
    else
    {
      playerCamera.enabled = false;
      playerAudio.enabled = false;
      playerCameraObject.SetActive(false);
      lobbyCamera.enabled = true;
      lobbyAudio.enabled = true;
    }
  }

  public void PlayerControlsActive(bool active)
  {
    if (inputManager == null) inputManager = GetComponent<InputManager>();
    if (active)
    {
      if (!inputManager.enabled)
      {
        inputManager.enabled = true;
        if (!inputManager.initialized) inputManager.Initialize();
        // inputManager.inputs.controls.Enable();
      }
    }
    else
    {
      // inputManager.initialized = false;
      inputManager.enabled = false;
    }
  }

  public override void Initialize()
  {
    playerCameraObject = playerCamera.gameObject;

    // increment player count
    CmdEditPlayerNumber(1);

    // particleContainer = GetComponent<ParticleContainer>();
    // creatureGenerator = GetComponentInChildren<CreatureGenerator>(); // should only be 1
    particleContainer = GetComponent<ParticleContainer>();
    inputManager = GetComponent<InputManager>();
    creatureGenerator = GetComponentInChildren<CreatureGenerator>();
    int[] tempLimbIds = creatureGenerator.RandomizeCreature();

    if (isLocalPlayer)
    {
      CmdSyncLimbs(tempLimbIds);
      // cachedEquippedLimbs = tempLimbIds;
      // creatureGenerator.RandomizeCreature(); // make pler
      // turn off lobby camera and turn on own
      // if (!Scene.name.Contains("lobby") && !Scene.name.Contains("Lobby"))
      // {
      PlayerControlsActive(true);
      PlayerCameraActive(true);
      // }

      // get go button
      GameObject readyButtonObj = GameObject.Find("ReadyButton");
      GameObject notReadyButtonObj = GameObject.Find("UnReadyButton");
      // if (readyButtonObj == null || notReadyButtonObj == null || allPlayersReadyFlag) PlayerReady(); // auto ready if cant find buttons
      if (readyButtonObj != null && notReadyButtonObj != null)
      {
        readyButtonObj.GetComponent<Button>().onClick.AddListener(PlayerReady);
        notReadyButtonObj.GetComponent<Button>().onClick.AddListener(PlayerNotReady);
      }

      // get network manager to swap scenes
      netManager = FindObjectOfType<GameNetworkManager>();
      for(int i = 0; i < creatureGenerator.limbs.Length; i++)
      {
        netManager.spawnPrefabs.Add(creatureGenerator.limbs[i].gameObject);
      }

    }
    else
    {
      // creatureGenerator.RandomizeCreature(); // make pler
      PlayerControlsActive(false);
      PlayerCameraActive(false);
    }

    initialized = true;
  }

  public void Update()
  {
    if(initialized && isLocalPlayer && NetworkClient.ready)
    {
      if (inputManager.inputsChangedFlag)
      {
        inputManager.inputsChangedFlag = false;
        Debug.Log("registered input change from player: " + netId.ToString());
        CmdSendInputsToServer(inputManager.PackageInputs());
      }
    }

    if (refreshLimbs && isLocalPlayer)
    {
      refreshLimbs = false;
      cachedEquippedLimbs = new int[creatureGenerator.equippedLimbIds.Length];
    }
    // if(!CheckEquality(cachedEquippedLimbs, creatureGenerator.equippedLimbIds))
    // {
    //   print("detected limb change, sending updates...");
    // }
    if (initialized && !CheckEquality(cachedEquippedLimbs, creatureGenerator.equippedLimbIds) && isLocalPlayer)
    {
      cachedEquippedLimbs = creatureGenerator.equippedLimbIds;
      CmdSyncLimbs(creatureGenerator.equippedLimbIds);
      // dont do command, just update creature gen to match
      // if (!isLocalPlayer) creatureGenerator.SetLimbIds(cachedEquippedLimbs);
      // CmdSyncLimbs(cachedEquippedLimbs);
      // SendLimbIds(cachedEquippedLimbs);
    }

    if (isLocalPlayer && !allPlayersReadyFlag)
    {
      // Check if all player managers are ready
      PlayerManager[] managers = FindObjectsOfType<PlayerManager>();
      bool tempFlag = true; // innocent till proven guilty
      foreach(PlayerManager mng in managers)
      {
        if (!mng.indPlayerReadyFlag)
        {
          tempFlag = false;
        }
      }
      if (tempFlag)
      {
        if (!allPlayersReadyFlag)
        {
          allPlayersReadyFlag = true;
          // turn on player cameras and turn off lobby camera
          CmdActivateAllPlayers();
          CmdWaitForSceneLoad();
          // start gam
          if (isServer)
          {
            netManager.LateLoadGameScene("SampleScene");
          }
        }
      }
    }
  }

  [Command]
  public void CmdSendInputsToServer(bool[] playerInputs)
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

  [ClientRpc]
  private void SetClientReady(bool set)
  {
    indPlayerReadyFlag = set;
  }

  [Command]
  public void CmdSetClientReady(bool set)
  {
    SetClientReady(set);
  }

  public void PlayerReady() // only called from local player
  {
    Debug.Log("player clicked ready button");
    CmdSetClientReady(true);
  }

  public void PlayerNotReady()
  {
    Debug.Log("player clicked not ready button");
    CmdSetClientReady(false);
  }

  [Command(requiresAuthority = false)]
  public void CmdEditPlayerNumber(int inc)
  {
    EditPlayerNumber(inc);
  }

  [ClientRpc]
  private void EditPlayerNumber(int inc)
  {
    if (isLocalPlayer) numPlayers += inc;
  }

  [ClientRpc] // called when all players ready
  private void ActivateAllPlayers()
  {
    allPlayersReadyFlag = true;
    if (isLocalPlayer)
    {
      PlayerControlsActive(true);
      PlayerCameraActive(true);
    }
  }

  [Command]
  public void CmdActivateAllPlayers()
  {
    ActivateAllPlayers();
  }

  [Command]
  public void CmdWaitForSceneLoad()
  {
    WaitForSceneLoad(false);
  }
  [ClientRpc]
  private void WaitForSceneLoad(bool active)
  {
    // turn off self, and network manager will turn back on, turn on loading screen
    if (isLocalPlayer)
    {
      // lobbyCameraObject.SetActive(true);
      lobbyCamera.enabled = true;
      lobbyAudio.enabled = true;
      netManager.loadingMenu.SetActive(true);
      gameObject.SetActive(active);
    }
  }

  public override void OnStartClient()
  {
    Initialize();

    // check if we're in lobby
    Scene currentScene = SceneManager.GetActiveScene();
    if (currentScene.name.Contains("Lobby") || currentScene.name.Contains("lobby"))
    {
      PlayerCameraActive(false);
      PlayerControlsActive(false);
    }
    else
    {
      allPlayersReadyFlag = true;
      indPlayerReadyFlag = true;
    }

    if (isLocalPlayer)
    {
      gameObject.name = $"local player {numPlayers}";
      // Debug.Log("starting local player");
    }
    else
    {
      gameObject.name = $"remote player {numPlayers}";
      // Debug.Log("starting remote player");
    }

    // make it so players dont get destoyed on load
    if(isLocalPlayer) refreshLimbs = true;
    DontDestroyOnLoad(this.gameObject);

  }

  public override void OnStopClient()
  {
    if (isLocalPlayer)
    {
      PlayerCameraActive(false);
    }
    if (creatureGenerator != null) creatureGenerator.Die(false);
    base.OnStopClient();
  }

  // public void OnEnable()
  // {
  //   Debug.Log($"player {gameObject.name} enabled");
  // }
  //
  // public void OnDisable()
  // {
  //   Debug.Log($"player {gameObject.name} disabled");
  // }

}
