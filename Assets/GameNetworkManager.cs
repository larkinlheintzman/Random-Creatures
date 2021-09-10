using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
// using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{

  // Set by UI element UsernameInput OnValueChanged
  public string PlayerName { get; set; }
  public TMP_InputField inputField;
  public string levelSceneName = "SampleScene";

  public GameObject loadingMenu;

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

  public override void OnClientSceneChanged(NetworkConnection conn)
  {
    // always become ready.
    base.OnClientSceneChanged(conn);

    NetworkClient.localPlayer.gameObject.SetActive(true);
    if (NetworkServer.active)
    {
      // spawn some fellas in
      for (int i = 0; i < 1; i++)
      {
        // Debug.Log("spawning enemy...");
        // GameObject newEnemy = Instantiate(spawnPrefabs[0], Vector3.zero + Random.value*10f*Vector3.right + Random.value*10f*Vector3.forward, new Quaternion());
        // NetworkServer.Spawn(newEnemy);
      }
    }

  }


}
