using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyParticles : MonoBehaviour
{
  CreatureGenerator gen;
  public void Initialize(CreatureGenerator generator)
  {
    gen = generator;
  }
}
