using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
  public float maxHealth = 10f;
  public float currentHealth = 10f;
  public float recoveryRate = 0.01f;
  public Slider bar;
  public CreatureGenerator gen;

  public void Initialize(CreatureGenerator generator, Slider slider)
  {
    gen = generator;
    bar = slider;
    bar.value = currentHealth/maxHealth;
  }

  public void Damage(float dmg)
  {
    currentHealth = currentHealth - dmg;
    bar.value = currentHealth/maxHealth;
    if (currentHealth <= 0.0f) // if ded
    {
      gen.Die();
    }
  }

  public void Update()
  {
    // recover some bar
    currentHealth += recoveryRate*Time.fixedDeltaTime;
    if (currentHealth < 0.0f)
    {
      currentHealth = 0.0f;
    }
    else if (currentHealth > maxHealth)
    {
      currentHealth = maxHealth;
    }
    bar.value = currentHealth/maxHealth;
  }

}
