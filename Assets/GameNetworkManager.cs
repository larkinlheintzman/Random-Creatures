using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class GameNetworkManager : NetworkManager
{

  // Set by UI element UsernameInput OnValueChanged
  public string PlayerName { get; set; }
  public TMP_InputField inputField;

  // Called by UI element NetworkAddressInput.OnValueChanged
  public void SetHostname()
  {
      networkAddress = inputField.text;
  }
  // public override void OnClientConnect(NetworkConnection conn)
  // {
  //   Debug.Log("pler joined!");
  // }
  //
  // public override void OnClientDisconnect(NetworkConnection conn)
  // {
  //   base.OnClientDisconnect(conn);
  //   Debug.Log("pler left :(");
  // }
  //
  // public override void OnStartClient()
  // {
  //   Debug.Log("client started!");
  // }

  // public override void OnServerAddPlayer(NetworkConnection conn)
  // {
  //     Transform startPos = GetStartPosition();
  //     GameObject player = startPos != null
  //         ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
  //         : Instantiate(playerPrefab);
  //     // instantiating a "Player" prefab gives it the name "Player(clone)"
  //     // => appending the connectionId is WAY more useful for debugging!
  //     player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
  //
  //     // add player to list
  //     PlayerManager playerManager = player.GetComponent<PlayerManager>();
  //     playerList.Add(playerManager);
  //     // Debug.Log("player list size: " + playerList.Count);
  //     Debug.Log("player adding player w netID: " + player.GetComponent<NetworkIdentity>().netId);
  //     playerManager.playerId = playerList.Count - 1;
  //     playerManager.playerList = playerList; // copy over list of players??
  //
  //     // give player some authority over itself..? maybe?
  //     NetworkServer.AddPlayerForConnection(conn, player);
  // }

}
