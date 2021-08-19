using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class InputManager : MonoBehaviour
{

  public CharacterInputs inputs;
  public Vector3 movementInput = Vector3.zero;
  public bool movementPressed = false;
  public bool isLocalPlayer = true;
  public bool inputsChangedFlag = false;
  public bool[] previousInputs = new bool[16];

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
  public bool initialized = false;

  public static bool CheckEquality<T>(T[] first, T[] second)
  {
    return first.SequenceEqual(second);
  }

  public bool[] PackageInputs()
  {
    // put bools into array to be passed around, needs to be generalized probably
    bool[] inputArray = new bool[16];
    inputArray[0] = inventoryPressed;
    inputArray[1] = aimPressed;
    inputArray[2] = slidePressed;
    inputArray[3] = shootPressed;
    inputArray[4] = punchPressed;
    inputArray[5] = runPressed;
    inputArray[6] = jumpPressed;
    inputArray[7] = rollPressed;
    inputArray[8] = inventory0Pressed;
    inputArray[9] = inventory1Pressed;
    inputArray[10] = inventory2Pressed;
    inputArray[11] = inventory3Pressed;
    inputArray[12] = inventory4Pressed;
    inputArray[13] = inventoryOpen;
    inputArray[14] = aiming;
    inputArray[15] = isSliding;
    return inputArray;
  }

  public void ReadInputs(bool[] inputArray)
  {
    inventoryPressed  = inputArray[0];
    aimPressed        = inputArray[1];
    slidePressed      = inputArray[2];
    shootPressed      = inputArray[3];
    punchPressed      = inputArray[4];
    runPressed        = inputArray[5];
    jumpPressed       = inputArray[6];
    rollPressed       = inputArray[7];
    inventory0Pressed = inputArray[8];
    inventory1Pressed = inputArray[9];
    inventory2Pressed = inputArray[10];
    inventory3Pressed = inputArray[11];
    inventory4Pressed = inputArray[12];
    inventoryOpen     = inputArray[13];
    aiming            = inputArray[14];
    isSliding         = inputArray[15];
  }

  public void Initialize()
  {
    inputs = new CharacterInputs();
    inputs.controls.Enable();

    if (isLocalPlayer) // only local players can move, but attacks and such go through
    {
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

    }

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

      inputs.controls.Enable();
      Cursor.lockState = CursorLockMode.Locked;
      initialized = true;
  }

  public void Update()
  {
    if (isLocalPlayer && initialized)
    {
      HandleSliding();
      HandleAiming();
      HandleInventory();
      bool[] currentInputs = PackageInputs();
      if (!CheckEquality(currentInputs, previousInputs)) inputsChangedFlag = true;
      // else inputsChangedFlag = false;
      previousInputs = currentInputs;
    }
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
      Cursor.lockState = CursorLockMode.None;
    }
    if (!inventoryPressed && inventoryOpen)
    {
      // orbitCam.distance = 2.0f*oldCameraDist;
      inventoryOpen = false;
      Cursor.lockState = CursorLockMode.Locked;
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
    if (isLocalPlayer && initialized)
    {
      inputs.controls.Disable();
      Cursor.lockState = CursorLockMode.None;
    }
  }


  void OnEnable ()
  {
    if (isLocalPlayer && initialized)
    {
      inputs.controls.Enable();
      Cursor.lockState = CursorLockMode.Locked;
    }
  }


}
