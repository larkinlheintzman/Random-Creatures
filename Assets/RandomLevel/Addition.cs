using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Addition : MonoBehaviour
{
  public enum AdditionType {top, bottom, side, corner, light};

  public AdditionType addType;
  public int depth = 0;
  public CityStarGenerator generator;
  public Block kid;

  public void Initialize(CityStarGenerator gen, int genDepth)
  {
    generator = gen;
    depth = genDepth;
    // pick random block from generator categories based on current rotation
    Block newBlock;
    int newAddOnIndex = 0;
    switch (addType)
    {
      case AdditionType.top:
        // addition pointing up
        // print("adding new block facing up");
        newAddOnIndex = generator.ProbPick(generator.verticleAddOns);
        newBlock = Instantiate(generator.verticleAddOns[newAddOnIndex].blk, transform).GetComponent<Block>();
        break;
      case AdditionType.side:
        // addition pointing sidwers
        newAddOnIndex = generator.ProbPick(generator.horizontalAddOns);
        newBlock = Instantiate(generator.horizontalAddOns[newAddOnIndex].blk, transform).GetComponent<Block>();
        break;

      default:
        // addition pointing sidewers
        newAddOnIndex = generator.ProbPick(generator.horizontalAddOns);
        newBlock = Instantiate(generator.horizontalAddOns[newAddOnIndex].blk, transform).GetComponent<Block>();
        break;
    }
    
    // newBlock.transform.parent = transform;

    newBlock.Initialize(generator, depth + 1, Vector3.zero);

  }

  public void OnDrawGizmos()
  {
    Gizmos.color = Color.red;

    //Draw a Ray forward from GameObject toward the hit
    //Draw a cube that extends to where the hit exists
    if (kid != null)
    {
      Gizmos.DrawWireCube(kid.container.bounds.center, kid.container.bounds.extents*2);
    }

    Gizmos.DrawIcon(transform.position, "Light Gizmo.tiff", true);

  }

}
