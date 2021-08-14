using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class IsoCamera : MonoBehaviour
{

  public Transform player;
  public float rotationSpeed;
  public float distance;
  public Vector2 orbitAngles = new Vector2(45.0f, 0.0f);

  private CharacterInputs input;

  void Awake()
  {
    input = new CharacterInputs();
    input.controls.CameraRotate.performed += ctx =>
    {
      float dirInput = ctx.ReadValue<float>();
      orbitAngles += new Vector2(0.0f, dirInput*90.0f);
    };
  }

  // Update is called once per frame
  void Update()
  {
    Quaternion lookRotation;
    lookRotation = Quaternion.Euler(orbitAngles);

    Vector3 lookDirection = lookRotation * Vector3.forward;
    Vector3 lookPosition = player.position - lookDirection * distance;

    transform.SetPositionAndRotation(lookPosition, lookRotation);
  }


  void OnEnable ()
  {
    input.controls.Enable();
    // lock cursor to window
    // Cursor.lockState = CursorLockMode.Locked;
  }

  void OnDisable ()
  {
    input.controls.Disable();
    // Cursor.lockState = CursorLockMode.None;
  }

}
