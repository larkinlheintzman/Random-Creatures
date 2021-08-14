using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideWalkAddOn : BlockAddOn
{
  public override void Initialize(RandomCityGenerator generator, Block block, Vector3 pos, Vector3 dir)
  {
    
    // turn to point at dir
    transform.rotation = Quaternion.LookRotation(dir,  block.transform.up);
    transform.position = pos + dir*generator.sideWalkWidth/2f + block.transform.up*0.5f;
    transform.localScale = new Vector3((block.dims.x + block.dims.z)/2f + generator.sideWalkWidth*2.0f, generator.sideWalkThickness, generator.sideWalkWidth);
    // put on block
    transform.parent = block.transform;

  }
}
