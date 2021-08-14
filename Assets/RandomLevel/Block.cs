using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
  public Vector3 dims = new Vector3(1f,1f,1f);
  public bool isTrigger;
  public Collider col;
  public RandomCityGenerator generator;
  public virtual void Initialize(RandomCityGenerator generator)
  {
    col = gameObject.GetComponent<Collider>();

    // clear out children in case of re init
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

    List<Vector3> sideDirList = new List<Vector3> {transform.forward, -transform.forward, transform.right, -transform.right};
    int counter = 0;
    // do side neighbor checking
    foreach(Vector3 dir in sideDirList)
    {
      Vector3 normalizedDir = dir.normalized;
      // pick add on subclass stuff?
      if (generator.sideWalkPrefabs.Length > 0)
      {
        // List<SideWalkAddOn> generator.sideWalkPrefabs = generator.addOnPrefabs.OfType<SideWalkAddOn>();
        int sideWalkNum = generator.sideWalkPrefabs.Length;
        SideWalkAddOn sideWalk = Instantiate(generator.sideWalkPrefabs.PickOne()); // pick random sidewalk
        Vector3 pos = GetPositionOnFace(counter, 0.0f, -0.5f);
        sideWalk.Initialize(generator, this, pos, normalizedDir);
      }

      // add some bits and bobs to buildings
      if (generator.addOnPrefabs.Length > 0)
      {
        // pick add on
        BlockAddOn addOn = generator.addOnPrefabs.PickOne();
        // cast ray out
        Vector3 candiateFacePosition = GetPositionOnFace(counter, Random.value - 0.5f, Random.value - 0.5f);
        RaycastHit rcHit;

        // getting width here is a little sketchy because we havent instantiated yet
        if (!Physics.Raycast(candiateFacePosition, normalizedDir, out rcHit, generator.addOnCheckDistance, generator.blockLayerMask))
        {
          BlockAddOn newAddOn = Instantiate(addOn);
          newAddOn.Initialize(generator, this, candiateFacePosition, normalizedDir);
        }

      }
      counter += 1;
    }

  }

  public virtual Vector3 GetPositionOnFace(int faceId, float xOffset, float yOffset)
  {
    // foward
    if (faceId == 0) return transform.position + transform.forward*dims.z/2f + xOffset*transform.right*dims.x + yOffset*transform.up*dims.y;
    // back
    if (faceId == 1) return transform.position - transform.forward*dims.z/2f + xOffset*transform.right*dims.x + yOffset*transform.up*dims.y;

    // right
    if (faceId == 2) return transform.position + transform.right*dims.x/2f + xOffset*transform.forward*dims.z + yOffset*transform.up*dims.y;
    // left
    if (faceId == 3) return transform.position - transform.right*dims.x/2f + xOffset*transform.forward*dims.z + yOffset*transform.up*dims.y;

    // top
    if (faceId == 4) return transform.position + transform.up*dims.y/2f + xOffset*transform.right*dims.x + yOffset*transform.forward*dims.z;
    // bottom
    if (faceId >= 5) return transform.position - transform.up*dims.y/2f + xOffset*transform.right*dims.x + yOffset*transform.forward*dims.z;

    return Vector3.zero;
  }

}
