using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
  [SerializeField]
  public int depth = 0;
  public Collider container;
  public bool randomScale = true;
  public Addition[] additionSpots;
  // public Transform
  public CityStarGenerator generator;
  // public Collider col;

  public virtual void Initialize(CityStarGenerator generator, int genDepth, Vector3 localScale)
  {
    depth = genDepth;
    // col = gameObject.GetComponent<Collider>();

    // match scale of generator
    this.generator = generator;
    if (randomScale)
    {
      // if localScale is zero, we handle scaling
      if (Vector3.Distance(Vector3.zero, localScale) != 0) transform.localScale = localScale;
      else
      {
        Vector3 newScale = Vector3.zero;
        float noiseVal = (2f*Random.value - 1f);
        newScale.x = generator.blockSizeScale.x*noiseVal;
        noiseVal = (2f*Random.value - 1f);
        newScale.y = generator.blockSizeScale.y*noiseVal;
        noiseVal = (2f*Random.value - 1f);
        newScale.z = generator.blockSizeScale.z*noiseVal;
        transform.localScale += newScale;
        // general branch scaling stuff
        transform.localScale = transform.localScale*generator.childSizeScaler;

        // additionally wiggle about x and z and y axis
        // blk.transform.rotation = Quaternion.Slerp(blk.transform.rotation, b, t)
        Quaternion upRotation = Quaternion.AngleAxis(generator.blockRotationNoise.y*(2f*Random.value - 1f), transform.up);
        Quaternion rightRotation = Quaternion.AngleAxis(generator.blockRotationNoise.x*(2f*Random.value - 1f), transform.right);
        Quaternion forwardRotation = Quaternion.AngleAxis(generator.blockRotationNoise.z*(2f*Random.value - 1f), transform.forward);

        transform.rotation = transform.rotation*upRotation*rightRotation*forwardRotation;
      }
    }
    else
    {
      // reset scale to unity
      Transform tempParent = transform.parent;
      transform.parent = null;
      transform.localScale = Vector3.one;
      transform.parent = tempParent;
    }

    // check collisions
    bool hitBool = false;
    container.enabled = false;
    Collider[] hitColliders = Physics.OverlapBox(container.bounds.center, container.bounds.extents, transform.rotation, generator.blockLayerMask);
    if (hitColliders.Length > 0) hitBool = true;
    if (!hitBool && depth + 1 < generator.maxDepth)
    {
      container.enabled = true;
    }
    else
    {
      print($"hit other box, turning off");
      gameObject.SetActive(false);
      // Destroy(gameObject);
      return;
      // kid.gameObject.SetActive(false);
    }

    // clear out children in case of re init
    foreach(Addition spot in additionSpots)
    {
      while(spot.transform.childCount > 0)
      {
        foreach (Transform child in spot.transform)
        {
          GameObject.DestroyImmediate(child.gameObject);
        }
      }
    }


    if (additionSpots.Length > 0)
    {
      for (int i = 0; i < additionSpots.Length; i++) {
        if (additionSpots[i] != null) additionSpots[i].Initialize(generator, depth);
      }
    }

  }

}
