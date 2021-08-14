using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    public ParticleSystem particles;
    public bool deleteOnEnd = true;
    public bool playingFlag = false;

    public void Awake()
    {
      particles = GetComponent<ParticleSystem>();
      particles.Play();
      playingFlag = true;
    }

    public void Update()
    {
      if (playingFlag)
      {
        if (!particles.isPlaying)
        {
          Object.Destroy(this.gameObject);
        }
      }
    }
}
