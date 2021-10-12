using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockAddOn : MonoBehaviour
{
  public Vector3 dims = new Vector3(1f,1f,1f);
  public bool isTrigger;
  public Collider col;
  public CityStarGenerator generator;

  public virtual void Initialize(CityStarGenerator generator, Block block, Vector3 pos, Vector3 dir)
  {

    col = gameObject.GetComponent<Collider>();
    this.generator = generator;
    transform.localScale = dims;
    col.isTrigger = isTrigger;

    // turn to point at dir
    transform.rotation = Quaternion.LookRotation(dir,  block.transform.up);
    transform.position = pos + dir*GetWidth()/2f;
    // put on block
    transform.parent = block.transform;

  }

  public virtual float GetWidth()
  {
    return dims.z; // stupid
  }

  public virtual void SetRandomScale()
  {
    dims = (Random.value + 1f)*dims;
  }

}
