using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
  [SerializeField]
  public float lookDistance = 10.0f;
  [SerializeField]
  public float chaseDistance = 8.0f;

  public Transform target;
  public NavMeshAgent agent;
  public CreatureGenerator generator;
  public Manager enemyManager;

  public PlayerManager[] players;

  public void Initialize()
  {
    target = new GameObject().transform; // needs to be more general
    agent = GetComponent<NavMeshAgent>();
    enemyManager = GetComponent<Manager>();
    generator = GetComponent<CreatureGenerator>();
    generator.RandomizeCreature();
  }

  void Update()
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
    else if (distance < lookDistance) {
      agent.transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
    }

  }

}
