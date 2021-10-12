using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
// using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{

  // Set by UI element UsernameInput OnValueChanged
  public float randomEnemyInterval = 30.0f;
  public float randomEnemyDistance = 20.0f;
  private float randomEnemySpawnTime = -1.0f;

  public string PlayerName { get; set; }
  public TMP_InputField inputField;
  public string levelSceneName = "SampleScene";
  public int sceneIndexCounter = 0;
  public bool autoStart;

  public GameObject loadingMenu;

  public override void Start()
  {
    base.Start();
    if (autoStart)
    {
      StartHost();

      if (NetworkServer.active)
      {
        // spawn some fellas in
        // for (int i = 0; i < 2; i++)
        // {
        //   Debug.Log("spawning enemy...");
        //   GameObject newEnemy = Instantiate(spawnPrefabs[0], Vector3.zero + Random.value*5f*Vector3.right + Random.value*5f*Vector3.forward, new Quaternion());
        //   NetworkServer.Spawn(newEnemy);
        // }
      }

    }
  }

  public void SetHostname()
  {
      networkAddress = inputField.text;
  }

  public void LateLoadGameScene(string sceneName)
  {
    StartCoroutine(IELateLoadGameScene(sceneName));
  }

  public IEnumerator IELateLoadGameScene(string sceneName) {
    // load game scene after 0.5 second
    yield return new WaitForSeconds(1.0f);
    print("scene charge starting");
    ServerChangeScene(sceneName);
  }

  public void SpawnEnemy(Vector3 worldPos)
  {
    Debug.Log("server spawning enemy...");
    GameObject newEnemy = Instantiate(spawnPrefabs[0], worldPos, new Quaternion());
    NetworkServer.Spawn(newEnemy);
  }

  public override void OnClientSceneChanged(NetworkConnection conn)
  {
    // always become ready.
    base.OnClientSceneChanged(conn);

    NetworkClient.localPlayer.gameObject.SetActive(true);
    if (NetworkServer.active)
    {
      // spawn some fellas in
      // for (int i = 0; i < 2; i++)
      // {
      //   Vector3 randPos = Vector3.zero + Random.value*5f*Vector3.right + Random.value*5f*Vector3.forward;
      //   singleton.SpawnEnemy(randPos);
      // }
    }
  }

    public void Update()
    {
      // summon some enemies around players
      PlayerManager[] players = FindObjectsOfType<PlayerManager>();
      // float maxDist = Mathf.Infinity;
      if (randomEnemySpawnTime < Time.time && false)
      {
        foreach(PlayerManager pler in players)
        {
          SpawnEnemy(pler.creatureGenerator.transform.position + (Random.value - 0.5f)*randomEnemyDistance*Vector3.forward + (Random.value - 0.5f)*randomEnemyDistance*Vector3.right);
        }
        randomEnemySpawnTime = Time.time + randomEnemyInterval;
      }
    }

  }
