using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
  [SerializeField]
  public Rigidbody rb;
  [SerializeField]
  public Collider col;
  [SerializeField]
  public MeshRenderer meshRenderer;
  [SerializeField]
  public float accelerationTime = 0.01f;
  [SerializeField]
  public float timeAlive = 0.0f;
  [SerializeField]
  public float accelerationForce = 5f;
  [SerializeField]
  public float maxLifetime = 5f;
  [SerializeField]
  public Vector3 direction;
  [SerializeField]
  public bool initialized = false;
  [SerializeField]
  public bool destroyFlag = false;
  [SerializeField]
  public LayerMask layerMask;
  [SerializeField]
  public float damage = 0.5f;
  [SerializeField]
  public ParticleSystem trailParticles;
  public ParticleContainer particleContainer;

  public void Awake()
  {
    particleContainer = GameObject.Find("GameManager").GetComponent<ParticleContainer>();
  }

  public void Fire(Vector3 direction, LayerMask layerMask)
  {
    // transform.position = position;
    this.gameObject.name = "bullet";
    this.rb = GetComponent<Rigidbody>();
    this.col = GetComponent<Collider>();
    // this.meshRenderer = GetComponent<MeshRenderer>();

    this.col.isTrigger = true;
    this.layerMask = layerMask;
    this.initialized = true;
    this.destroyFlag = false;
    this.direction = direction;
    this.trailParticles.Play();
  }

  void OnTriggerEnter(Collider col)
  {
    if(initialized)
    {
      if(layerMask == (layerMask | 1 << col.gameObject.layer))
      {
        // Debug.Log("bullet impact detected!");
        // impactParticles.Play();
        particleContainer.PlayParticle(2, transform.position);
        Health health = col.gameObject.GetComponent<Health>();
        if (health != null)
        {
          health.Damage(damage);
        }
        destroyFlag = true;
      }
    }
  }

  // void OnTriggerExit(Collider col)
  // {
  //   if(initialized)
  //   {
  //     if(layerMask == (layerMask | 1 << col.gameObject.layer))
  //     {
  //       // Debug.Log("bullet deleted!");
  //       // Destroy(gameObject);
  //     }
  //   }
  // }

  void Update()
  {
    if(initialized)
    {
      if (timeAlive <= accelerationTime)
      {
        rb.AddForce(accelerationForce*direction);
      }

      if (timeAlive >= maxLifetime)
      {
        Destroy(gameObject);
      }

      if (destroyFlag)
      {
        // wait for particles to get done playing
        rb.isKinematic = true;
        col.enabled = false;
        meshRenderer.enabled = false;
        Destroy(gameObject);
      }

      timeAlive += Time.deltaTime;
    }


  }

}
