using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleContainer : MonoBehaviour
{

    public ParticleSystem[] effects;

    public void PlayParticle(int particleNumber, Vector3 pos, Transform trs = null)
    {
      if (effects != null && effects[particleNumber] != null)
      {
        // turn off playing if they were left on lol
        if (effects[particleNumber].isPlaying) effects[particleNumber].Stop();

        // ParticleSystem tempPart = Instantiate(effects[particleNumber], pos, new Quaternion()) as ParticleSystem;
        ParticleSystem tempPart = Instantiate(effects[particleNumber], pos, new Quaternion()) as ParticleSystem;
        GameObject tempObj = tempPart.gameObject;
        // parent to transform if it's not null
        if (trs != null)
        {
          tempObj.transform.parent = trs;
        }
        tempObj.AddComponent<ParticleEmitter>();
        // tempPart.Play();

      }
    }
}
