using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorBlock : Block
{
  public override void Initialize(RandomCityGenerator generator)
  {

    col = gameObject.GetComponent<Collider>();

    // foreach (Transform child in transform)
    while(transform.childCount > 0)
    {
      foreach (Transform child in transform)
      {
        GameObject.DestroyImmediate(child.gameObject);
      }
    }

    this.generator = generator;
    transform.localScale = dims;
    col.isTrigger = isTrigger;
  }
}
