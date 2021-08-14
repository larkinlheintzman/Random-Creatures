using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
  public float maxHealth;
  public float currentHealth;
  public int steps;
  public CreatureGenerator gen;

  // void Awake()
  // {
  //
  // }
  public void Initialize(CreatureGenerator generator)
  {
    maxHealth = 2f;
    currentHealth = 2f;
    steps = 4; // for later i guess
    gen = generator;
  }

  public void Damage(float dmg)
  {
    currentHealth = currentHealth - dmg;
    if (currentHealth <= 0.0f) // if ded
    {
      gen.Die();
      // return true;
    }
    // return false; // still kicking
  }

}
