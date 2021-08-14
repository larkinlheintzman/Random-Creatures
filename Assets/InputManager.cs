using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
  #region Singleton

  public static InputManager instance;
  public CharacterInputs inputs;
  public bool controlsEnabled = false;

  public Vector3 movementInput = Vector3.zero;
  public bool movementPressed = false;

  public bool inventoryPressed = false;
  public bool aimPressed = false;
  public bool slidePressed = false;
  public bool shootPressed = false;
  public bool punchPressed = false;
  public bool runPressed = false;
  public bool jumpPressed = false;
  public bool rollPressed = false;
  public bool inventory0Pressed = false;
  public bool inventory1Pressed = false;
  public bool inventory2Pressed = false;
  public bool inventory3Pressed = false;
  public bool inventory4Pressed = false;
  public bool inventoryOpen = false;
  public bool aiming = false;
  public bool isSliding = false;

  public void Awake()
  {

    instance = this;

    inputs = new CharacterInputs();
    inputs.controls.Enable();

    // directional inputs
    inputs.controls.ZAxis.performed += ctx =>
    {
      movementInput.z = ctx.ReadValue<float>();
      movementInput.Normalize();
      movementPressed = movementInput.x != 0 || movementInput.z != 0;
    };

    inputs.controls.XAxis.performed += ctx =>
    {
      movementInput.x = ctx.ReadValue<float>();
      movementInput.Normalize();
      movementPressed = movementInput.x != 0 || movementInput.z != 0;

    };

    inputs.controls.Inventory.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        inventoryPressed = true;
      }
      else
      {
        inventoryPressed = false;
      }
    };

    inputs.controls.Aim.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        aimPressed = true;
      }
      else
      {
        aimPressed = false;
      }
    };

    inputs.controls.InventoryButton0.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        inventory0Pressed = true;
      }
      else
      {
        inventory0Pressed = false;
      }
    };

    inputs.controls.InventoryButton1.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        inventory1Pressed = true;
      }
      else
      {
        inventory1Pressed = false;
      }
    };

    inputs.controls.InventoryButton2.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        inventory2Pressed = true;
      }
      else
      {
        inventory2Pressed = false;
      }
    };

    inputs.controls.InventoryButton3.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        inventory3Pressed = true;
      }
      else
      {
        inventory3Pressed = false;
      }
    };

    inputs.controls.InventoryButton4.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        inventory4Pressed = true;
      }
      else
      {
        inventory4Pressed = false;
      }
    };

    inputs.controls.Punch.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        punchPressed = true;
      }
      else
      {
        punchPressed = false;
      }
    };

    inputs.controls.Slide.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        slidePressed = true;
      }
      else
      {
        slidePressed = false;
      }
    };

    inputs.controls.Shoot.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        shootPressed = true;
      }
      else
      {
        shootPressed = false;
      }
    };

    inputs.controls.Run.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        runPressed = true;
      }
      else
      {
        runPressed = false;
      }
    };

    inputs.controls.Jump.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        jumpPressed = true;
      }
      else
      {
        jumpPressed = false;
      }
    };

    inputs.controls.Roll.performed += ctx =>
    {
      if (ctx.ReadValueAsButton())
      {
        rollPressed = true;
      }
      else
      {
        rollPressed = false;
      }
    };

    controlsEnabled = true;
  }

  public void Update()
  {
    HandleSliding();
    HandleAiming();
    HandleInventory();
  }

  public void HandleSliding()
  {
    if (slidePressed && !inventoryOpen)
    {
      isSliding = true;
    }
    else
    {
      isSliding = false;
    }
  }

  public void HandleInventory()
  {
    // do inventory if's
    if (inventoryPressed && !inventoryOpen && !aimPressed && !aiming)
    {
      // orbitCam.distance = 0.5f*oldCameraDist;
      inventoryOpen = true;
    }
    if (!inventoryPressed && inventoryOpen)
    {
      // orbitCam.distance = 2.0f*oldCameraDist;
      inventoryOpen = false;
    }
  }

  public void HandleAiming()
  {
    // do aiming if's
    if (aimPressed && !aiming && !inventoryPressed && !inventoryOpen)
    {
      aiming = true;
    }
    else if (!aimPressed && aiming) {
      aiming = false;
    }
  }

  void OnDisable ()
  {
    if (controlsEnabled)
    {
      inputs.controls.Disable();
      Cursor.lockState = CursorLockMode.None;
    }
  }


  void OnEnable ()
  {
    if (controlsEnabled)
    {
      inputs.controls.Enable();
      Cursor.lockState = CursorLockMode.Locked;
    }
  }

  #endregion

}
