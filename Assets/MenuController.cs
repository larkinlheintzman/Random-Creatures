using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MenuController : NetworkBehaviour
{
  public GameObject menuObject;

  [TargetRpc]
  public void ClientStartLobby(NetworkConnection conn)
  {
    // informs client that server has registered the connection
    Debug.Log("turning off menu");
    menuObject.SetActive(false);
  }

  [TargetRpc]
  public void ClientStopLobby(NetworkConnection conn)
  {
    Debug.Log("turning on menu");
    menuObject.SetActive(true);
  }
}
