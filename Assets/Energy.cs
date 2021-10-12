using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
  public float maxEnergy = 10f;
  public float currentEnergy = 10f;
  public float recoveryRate = 1f;
  public float emptyThreshold = 0.5f; // if current energy is below this, we're officially out of gas
  public bool hasGas = true;
  public Slider bar;
  public CreatureGenerator gen;

  public void Initialize(CreatureGenerator generator, Slider slider)
  {
    gen = generator;
    bar = slider;
    bar.value = currentEnergy/maxEnergy;
  }

  public void Consume(float csm)
  {
    currentEnergy = currentEnergy - csm;
    bar.value = currentEnergy/maxEnergy;
    if (currentEnergy < 0.0f)
    {
      hasGas = false; // damger
    }
  }

  public void Update()
  {
    // recover some bar
    currentEnergy += recoveryRate*Time.fixedDeltaTime;
    if (currentEnergy < 0.0f)
    {
      currentEnergy = 0.0f;
    }
    else if (currentEnergy > maxEnergy)
    {
      currentEnergy = maxEnergy;
      hasGas = true; // most assuradly
    }
    else if (currentEnergy > emptyThreshold)
    {
      hasGas = true;
    }
    bar.value = currentEnergy/maxEnergy;
  }

}
