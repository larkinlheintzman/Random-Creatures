using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

[RequireComponent(typeof(ParticleContainer)),RequireComponent(typeof(InputManager))]
public class EnemyManager : Manager
{
  public float lookDistance = 10.0f;
  public float chaseDistance = 8.0f;
  public float attackDistance = 2.0f;
  public float buttonCooldown = 2.0f;
  public float buttonCooldownCounter = 0.0f;
  // TODO add some better attack methods/AI here
  public PlayerManager[] players;

  public override void OnStartClient()
  {
    base.Initialize();

    target = new GameObject().transform; // needs to be more general
    target.gameObject.name = "playerTrackingTarget";
    target.parent = transform;
    agent = GetComponent<NavMeshAgent>();

    initialized = true;
  }

  public void ButtonPress(ref bool button)
  {
    if (buttonCooldownCounter > 0.0f)
    {
      buttonCooldownCounter -= Time.fixedDeltaTime;
      button = false;
      return;
    }

    button = true;
    buttonCooldownCounter = buttonCooldown;
  }

  public void Update()
  {
    if (initialized)
    {
      // find all player managers in scene, chase one
      players = FindObjectsOfType<PlayerManager>();
      float maxDist = Mathf.Infinity;
      foreach(PlayerManager pler in players)
      {
        float currDist = Vector3.Distance(transform.position, pler.creatureGenerator.transform.position);
        if (currDist < maxDist)
        {
          maxDist = currDist;
          target.position = pler.creatureGenerator.transform.position;
        }
      }

      float distance = Vector3.Distance(target.position, transform.position);
      if (distance < chaseDistance) {
        agent.SetDestination(target.position);
      }
      if (distance < lookDistance) lookEnabled = true;
      else lookEnabled = false;

      if (distance < attackDistance)
      {
        // take swings at player
        ButtonPress(ref inputManager.punchPressed);
      }

      // if (!CheckEquality(cachedLimbs, creatureGenerator.equippedLimbIds) && initialized && isLocalPlayer)
      // {
      //   cachedLimbs = creatureGenerator.equippedLimbIds;
      //   CmdSyncLimbs(cachedLimbs);
      // }

    }
  }
}
