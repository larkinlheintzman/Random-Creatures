using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  #region Singleton

  public static GameManager me;
  [HideInInspector]
  public ParticleContainer particleContainer;
  [HideInInspector]
  public InputManager inputManager;
  public Transform player;

  public void Awake()
  {
    me = this;
    me.inputManager = GetComponent<InputManager>();
    me.particleContainer = GetComponent<ParticleContainer>();
    me.player = player;
  }

  #endregion

}
