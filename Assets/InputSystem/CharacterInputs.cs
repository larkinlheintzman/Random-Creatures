// GENERATED AUTOMATICALLY FROM 'Assets/InputSystem/CharacterInputs.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @CharacterInputs : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @CharacterInputs()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""CharacterInputs"",
    ""maps"": [
        {
            ""name"": ""controls"",
            ""id"": ""210aa6e9-6203-491e-b955-dbef04413302"",
            ""actions"": [
                {
                    ""name"": ""Shoot"",
                    ""type"": ""Button"",
                    ""id"": ""674cf2d9-9845-4e84-a095-4542408d7395"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Slide"",
                    ""type"": ""Button"",
                    ""id"": ""0aa39347-c8ed-410b-9a68-41b3628b9e49"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""15b7efa5-dede-4993-b1e4-de050a6aa612"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""AltPunch"",
                    ""type"": ""Button"",
                    ""id"": ""20ed0dbe-3e13-4c77-ad4f-280824618170"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Ragdoll"",
                    ""type"": ""Button"",
                    ""id"": ""adccdf64-86e7-4d32-95e5-a1d173cfb462"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Punch"",
                    ""type"": ""Button"",
                    ""id"": ""954202cc-6cd6-45a8-a04a-ef7565c6a1b4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Roll"",
                    ""type"": ""Button"",
                    ""id"": ""fd1dd473-826d-4be2-895d-c8cd87ee9a9c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Run"",
                    ""type"": ""Button"",
                    ""id"": ""b506b996-8d33-4e26-a006-3caf350a6f42"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""XAxis"",
                    ""type"": ""Value"",
                    ""id"": ""761ddb3f-6679-4c69-9ca4-55a16ac55732"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ZAxis"",
                    ""type"": ""Value"",
                    ""id"": ""01a9fd13-0d08-45cc-abf7-ef7e29884b96"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=2)""
                },
                {
                    ""name"": ""CameraRotate"",
                    ""type"": ""Value"",
                    ""id"": ""42718c28-9783-4a7b-96ec-727acdacef36"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Inventory"",
                    ""type"": ""Button"",
                    ""id"": ""ef786181-bbdd-4e16-bab0-63d8188e6eb2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Aim"",
                    ""type"": ""Button"",
                    ""id"": ""b8ca0e61-94bf-4b12-9274-09228ca37485"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""InventoryButton1"",
                    ""type"": ""Button"",
                    ""id"": ""1d28786c-7cbc-4391-92bd-ead33e71a547"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""InventoryButton0"",
                    ""type"": ""Button"",
                    ""id"": ""70eb6d44-3795-4148-b02a-ef9bc5e94d01"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""InventoryButton2"",
                    ""type"": ""Button"",
                    ""id"": ""bc2cbfe6-caaf-4d4d-b532-1fa65c658e8c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""InventoryButton3"",
                    ""type"": ""Button"",
                    ""id"": ""f70bd4fb-f406-4d9d-aed3-4a7a495039d6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""InventoryButton4"",
                    ""type"": ""Button"",
                    ""id"": ""d7040146-f4cb-4b3a-b3a7-8364a4cd8559"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Zoom"",
                    ""type"": ""Value"",
                    ""id"": ""a8b2b631-ee6b-45c9-8322-54a2b1c284b0"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""2a9c2a6a-be38-4b5b-94f4-cf2abd7464e3"",
                    ""path"": ""1DAxis"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""4a1c87b3-8bb3-49f8-98e5-087d1324c9bf"",
                    ""path"": ""<Keyboard>/leftBracket"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""75dd7cdf-ca64-4ca9-b59d-a9b398a251e1"",
                    ""path"": ""<Keyboard>/rightBracket"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""91f8acb3-4f77-4c4c-8b4e-cdbabc6f3479"",
                    ""path"": ""1DAxis"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ZAxis"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""145d51d3-b00a-4937-8ba8-4c8c734f0c7d"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ZAxis"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""223fc76f-0071-4801-b181-816005bfc213"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ZAxis"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""9769c47a-fd16-48db-bcd7-cd78263be767"",
                    ""path"": ""1DAxis"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""XAxis"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""86e1ca46-80d8-4ea9-9039-98749c122c8a"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""XAxis"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""5ccd8b0c-b769-4b54-a29b-1ed45f5da1a9"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""XAxis"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""2d51680c-2221-41b4-92fa-4a2f84d77b96"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""24d578c7-3471-452f-a732-d100b65c2155"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Roll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ae971dc6-44ee-46b3-b090-3cfd87238886"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Punch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bea9074b-0bf6-469b-a3e7-63646dc42c98"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Ragdoll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""669706f1-43f9-4466-a845-726729f50224"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AltPunch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""19dc6635-385f-4c1d-b950-ddef00be9308"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""706bdf1f-1c5c-4884-a99b-e5d051d1c075"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""86633a0c-9802-47be-bb00-b475a0a689cf"",
                    ""path"": ""<Keyboard>/i"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Inventory"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""06dde86d-ae47-4c0c-ab4d-7ff6485338f2"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Aim"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""da26d842-59c1-4949-8d78-8946aee10e1e"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InventoryButton1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4f3bf9fc-49c9-4d2b-a50f-c5e22433a74e"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InventoryButton2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e29c4853-16a3-4429-ab36-9bc7d3004d56"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InventoryButton3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""01cf7385-764a-459f-b423-527357a0c916"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InventoryButton4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ce83be77-9409-469b-aee4-18ac35703cbe"",
                    ""path"": ""<Keyboard>/0"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""InventoryButton0"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""241f61a9-c759-4329-8e76-779bbd72268e"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": ""Press(behavior=2)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Slide"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c53468ac-3dd8-4941-8c27-3a7fcf0d9a77"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // controls
        m_controls = asset.FindActionMap("controls", throwIfNotFound: true);
        m_controls_Shoot = m_controls.FindAction("Shoot", throwIfNotFound: true);
        m_controls_Slide = m_controls.FindAction("Slide", throwIfNotFound: true);
        m_controls_Jump = m_controls.FindAction("Jump", throwIfNotFound: true);
        m_controls_AltPunch = m_controls.FindAction("AltPunch", throwIfNotFound: true);
        m_controls_Ragdoll = m_controls.FindAction("Ragdoll", throwIfNotFound: true);
        m_controls_Punch = m_controls.FindAction("Punch", throwIfNotFound: true);
        m_controls_Roll = m_controls.FindAction("Roll", throwIfNotFound: true);
        m_controls_Run = m_controls.FindAction("Run", throwIfNotFound: true);
        m_controls_XAxis = m_controls.FindAction("XAxis", throwIfNotFound: true);
        m_controls_ZAxis = m_controls.FindAction("ZAxis", throwIfNotFound: true);
        m_controls_CameraRotate = m_controls.FindAction("CameraRotate", throwIfNotFound: true);
        m_controls_Inventory = m_controls.FindAction("Inventory", throwIfNotFound: true);
        m_controls_Aim = m_controls.FindAction("Aim", throwIfNotFound: true);
        m_controls_InventoryButton1 = m_controls.FindAction("InventoryButton1", throwIfNotFound: true);
        m_controls_InventoryButton0 = m_controls.FindAction("InventoryButton0", throwIfNotFound: true);
        m_controls_InventoryButton2 = m_controls.FindAction("InventoryButton2", throwIfNotFound: true);
        m_controls_InventoryButton3 = m_controls.FindAction("InventoryButton3", throwIfNotFound: true);
        m_controls_InventoryButton4 = m_controls.FindAction("InventoryButton4", throwIfNotFound: true);
        m_controls_Zoom = m_controls.FindAction("Zoom", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // controls
    private readonly InputActionMap m_controls;
    private IControlsActions m_ControlsActionsCallbackInterface;
    private readonly InputAction m_controls_Shoot;
    private readonly InputAction m_controls_Slide;
    private readonly InputAction m_controls_Jump;
    private readonly InputAction m_controls_AltPunch;
    private readonly InputAction m_controls_Ragdoll;
    private readonly InputAction m_controls_Punch;
    private readonly InputAction m_controls_Roll;
    private readonly InputAction m_controls_Run;
    private readonly InputAction m_controls_XAxis;
    private readonly InputAction m_controls_ZAxis;
    private readonly InputAction m_controls_CameraRotate;
    private readonly InputAction m_controls_Inventory;
    private readonly InputAction m_controls_Aim;
    private readonly InputAction m_controls_InventoryButton1;
    private readonly InputAction m_controls_InventoryButton0;
    private readonly InputAction m_controls_InventoryButton2;
    private readonly InputAction m_controls_InventoryButton3;
    private readonly InputAction m_controls_InventoryButton4;
    private readonly InputAction m_controls_Zoom;
    public struct ControlsActions
    {
        private @CharacterInputs m_Wrapper;
        public ControlsActions(@CharacterInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @Shoot => m_Wrapper.m_controls_Shoot;
        public InputAction @Slide => m_Wrapper.m_controls_Slide;
        public InputAction @Jump => m_Wrapper.m_controls_Jump;
        public InputAction @AltPunch => m_Wrapper.m_controls_AltPunch;
        public InputAction @Ragdoll => m_Wrapper.m_controls_Ragdoll;
        public InputAction @Punch => m_Wrapper.m_controls_Punch;
        public InputAction @Roll => m_Wrapper.m_controls_Roll;
        public InputAction @Run => m_Wrapper.m_controls_Run;
        public InputAction @XAxis => m_Wrapper.m_controls_XAxis;
        public InputAction @ZAxis => m_Wrapper.m_controls_ZAxis;
        public InputAction @CameraRotate => m_Wrapper.m_controls_CameraRotate;
        public InputAction @Inventory => m_Wrapper.m_controls_Inventory;
        public InputAction @Aim => m_Wrapper.m_controls_Aim;
        public InputAction @InventoryButton1 => m_Wrapper.m_controls_InventoryButton1;
        public InputAction @InventoryButton0 => m_Wrapper.m_controls_InventoryButton0;
        public InputAction @InventoryButton2 => m_Wrapper.m_controls_InventoryButton2;
        public InputAction @InventoryButton3 => m_Wrapper.m_controls_InventoryButton3;
        public InputAction @InventoryButton4 => m_Wrapper.m_controls_InventoryButton4;
        public InputAction @Zoom => m_Wrapper.m_controls_Zoom;
        public InputActionMap Get() { return m_Wrapper.m_controls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ControlsActions set) { return set.Get(); }
        public void SetCallbacks(IControlsActions instance)
        {
            if (m_Wrapper.m_ControlsActionsCallbackInterface != null)
            {
                @Shoot.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnShoot;
                @Shoot.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnShoot;
                @Shoot.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnShoot;
                @Slide.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSlide;
                @Slide.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSlide;
                @Slide.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnSlide;
                @Jump.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnJump;
                @AltPunch.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnAltPunch;
                @AltPunch.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnAltPunch;
                @AltPunch.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnAltPunch;
                @Ragdoll.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRagdoll;
                @Ragdoll.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRagdoll;
                @Ragdoll.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRagdoll;
                @Punch.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnPunch;
                @Punch.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnPunch;
                @Punch.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnPunch;
                @Roll.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRoll;
                @Roll.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRoll;
                @Roll.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRoll;
                @Run.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRun;
                @Run.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRun;
                @Run.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnRun;
                @XAxis.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnXAxis;
                @XAxis.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnXAxis;
                @XAxis.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnXAxis;
                @ZAxis.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnZAxis;
                @ZAxis.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnZAxis;
                @ZAxis.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnZAxis;
                @CameraRotate.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnCameraRotate;
                @CameraRotate.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnCameraRotate;
                @CameraRotate.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnCameraRotate;
                @Inventory.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventory;
                @Inventory.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventory;
                @Inventory.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventory;
                @Aim.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnAim;
                @Aim.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnAim;
                @Aim.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnAim;
                @InventoryButton1.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton1;
                @InventoryButton1.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton1;
                @InventoryButton1.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton1;
                @InventoryButton0.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton0;
                @InventoryButton0.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton0;
                @InventoryButton0.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton0;
                @InventoryButton2.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton2;
                @InventoryButton2.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton2;
                @InventoryButton2.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton2;
                @InventoryButton3.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton3;
                @InventoryButton3.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton3;
                @InventoryButton3.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton3;
                @InventoryButton4.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton4;
                @InventoryButton4.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton4;
                @InventoryButton4.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnInventoryButton4;
                @Zoom.started -= m_Wrapper.m_ControlsActionsCallbackInterface.OnZoom;
                @Zoom.performed -= m_Wrapper.m_ControlsActionsCallbackInterface.OnZoom;
                @Zoom.canceled -= m_Wrapper.m_ControlsActionsCallbackInterface.OnZoom;
            }
            m_Wrapper.m_ControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Shoot.started += instance.OnShoot;
                @Shoot.performed += instance.OnShoot;
                @Shoot.canceled += instance.OnShoot;
                @Slide.started += instance.OnSlide;
                @Slide.performed += instance.OnSlide;
                @Slide.canceled += instance.OnSlide;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @AltPunch.started += instance.OnAltPunch;
                @AltPunch.performed += instance.OnAltPunch;
                @AltPunch.canceled += instance.OnAltPunch;
                @Ragdoll.started += instance.OnRagdoll;
                @Ragdoll.performed += instance.OnRagdoll;
                @Ragdoll.canceled += instance.OnRagdoll;
                @Punch.started += instance.OnPunch;
                @Punch.performed += instance.OnPunch;
                @Punch.canceled += instance.OnPunch;
                @Roll.started += instance.OnRoll;
                @Roll.performed += instance.OnRoll;
                @Roll.canceled += instance.OnRoll;
                @Run.started += instance.OnRun;
                @Run.performed += instance.OnRun;
                @Run.canceled += instance.OnRun;
                @XAxis.started += instance.OnXAxis;
                @XAxis.performed += instance.OnXAxis;
                @XAxis.canceled += instance.OnXAxis;
                @ZAxis.started += instance.OnZAxis;
                @ZAxis.performed += instance.OnZAxis;
                @ZAxis.canceled += instance.OnZAxis;
                @CameraRotate.started += instance.OnCameraRotate;
                @CameraRotate.performed += instance.OnCameraRotate;
                @CameraRotate.canceled += instance.OnCameraRotate;
                @Inventory.started += instance.OnInventory;
                @Inventory.performed += instance.OnInventory;
                @Inventory.canceled += instance.OnInventory;
                @Aim.started += instance.OnAim;
                @Aim.performed += instance.OnAim;
                @Aim.canceled += instance.OnAim;
                @InventoryButton1.started += instance.OnInventoryButton1;
                @InventoryButton1.performed += instance.OnInventoryButton1;
                @InventoryButton1.canceled += instance.OnInventoryButton1;
                @InventoryButton0.started += instance.OnInventoryButton0;
                @InventoryButton0.performed += instance.OnInventoryButton0;
                @InventoryButton0.canceled += instance.OnInventoryButton0;
                @InventoryButton2.started += instance.OnInventoryButton2;
                @InventoryButton2.performed += instance.OnInventoryButton2;
                @InventoryButton2.canceled += instance.OnInventoryButton2;
                @InventoryButton3.started += instance.OnInventoryButton3;
                @InventoryButton3.performed += instance.OnInventoryButton3;
                @InventoryButton3.canceled += instance.OnInventoryButton3;
                @InventoryButton4.started += instance.OnInventoryButton4;
                @InventoryButton4.performed += instance.OnInventoryButton4;
                @InventoryButton4.canceled += instance.OnInventoryButton4;
                @Zoom.started += instance.OnZoom;
                @Zoom.performed += instance.OnZoom;
                @Zoom.canceled += instance.OnZoom;
            }
        }
    }
    public ControlsActions @controls => new ControlsActions(this);
    public interface IControlsActions
    {
        void OnShoot(InputAction.CallbackContext context);
        void OnSlide(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnAltPunch(InputAction.CallbackContext context);
        void OnRagdoll(InputAction.CallbackContext context);
        void OnPunch(InputAction.CallbackContext context);
        void OnRoll(InputAction.CallbackContext context);
        void OnRun(InputAction.CallbackContext context);
        void OnXAxis(InputAction.CallbackContext context);
        void OnZAxis(InputAction.CallbackContext context);
        void OnCameraRotate(InputAction.CallbackContext context);
        void OnInventory(InputAction.CallbackContext context);
        void OnAim(InputAction.CallbackContext context);
        void OnInventoryButton1(InputAction.CallbackContext context);
        void OnInventoryButton0(InputAction.CallbackContext context);
        void OnInventoryButton2(InputAction.CallbackContext context);
        void OnInventoryButton3(InputAction.CallbackContext context);
        void OnInventoryButton4(InputAction.CallbackContext context);
        void OnZoom(InputAction.CallbackContext context);
    }
}
