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

  // [HideInInspector]
  public Transform target;
  // [HideInInspector]
  public NavMeshAgent agent;

  void Start()
  {
    target = GameManager.me.player;
    agent = GetComponent<NavMeshAgent>();
  }

  void Update()
  {
    float distance = Vector3.Distance(target.position, transform.position);
    if (distance < chaseDistance) {
      agent.SetDestination(target.position);
    }
    else if (distance < lookDistance) {
      agent.transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
    }

  }

}
