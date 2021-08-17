using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameNetworkManager : NetworkManager
{

  public override void OnClientConnect(NetworkConnection conn)
  {
    Debug.Log("pler joined!");
  }

  public override void OnClientDisconnect(NetworkConnection conn)
  {
    Debug.Log("pler left :(");
  }

  public override void OnStartClient()
  {
    Debug.Log("client started!");
  }

}
