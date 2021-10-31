using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityWell : MonoBehaviour
{
  public float force = 10.0f;
  public float range = 500.0f;
  [Range(0f, 1f)]
  public float G = 1f;

  public void FixedUpdate()
  {
    Rigidbody[] masses = FindObjectsOfType<Rigidbody>();
    GravityWell[] wells = FindObjectsOfType<GravityWell>();
    foreach(Rigidbody m in masses)
    {
      // add gravity force to each
      // Rigidbody rb = m.gameObject.GetComponent<Rigidbody>();
      Vector3 offset = transform.position - m.transform.position;
      float r2 = offset.sqrMagnitude;
      if (r2 < range*range && m != null)
      {
        // only make us the up direction if we're the closest well
        float minDist = Mathf.Infinity;
        Vector3 closestOffset = Vector3.one;
        foreach(GravityWell w in wells)
        {
          float sqrDist = (m.transform.position - w.transform.position).sqrMagnitude;
          if (sqrDist < minDist)
          {
            minDist = sqrDist;
            closestOffset = m.transform.position - w.transform.position;
          }
        }
        // m.AddForce(offset.normalized*(G*(m.mass*mass)/r2)); old fart gravity eq
        // if we are closest, add force
        if (offset.sqrMagnitude == minDist)
        {
          m.AddForce(-force*closestOffset.normalized);
        }
        // check if there's a mass controller
        MassController tmpCtrl = m.gameObject.GetComponent<MassController>();
        if (tmpCtrl != null)
        {
          // testing something!
          tmpCtrl.localDown = (transform.position - tmpCtrl.transform.position).normalized;
        }
      }
    }
  }
}
