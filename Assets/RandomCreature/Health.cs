using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
  public float maxHealth;
  public float currentHealth;
  public CreatureGenerator gen;

  public void Initialize(CreatureGenerator generator)
  {
    maxHealth = 100f;
    currentHealth = 100f;
    gen = generator;
  }

  public void Damage(float dmg)
  {
    currentHealth = currentHealth - dmg;
    if (currentHealth <= 0.0f) // if ded
    {
      gen.Die();
    }
  }

}
