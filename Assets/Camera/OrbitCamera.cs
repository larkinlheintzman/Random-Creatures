using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
// using UnityEngine.Experimental.Rendering.HDPipeline;

public class OrbitCamera : MonoBehaviour {

	public bool initialized = false;
	[SerializeField]
	public RenderTexture pixelTexture;

	[SerializeField]
	public Volume ppVolume;

	[SerializeField]
	public Transform focus = default;

	[SerializeField, Range(1f, 20f)]
	public float distance = 15f;

	[SerializeField]
	public Vector2 minMaxDistance = new Vector2(5f, 15f);

	[SerializeField, Range(1f, 20f)]
	public float inventoryDistance = 10f;

	[SerializeField, Range(1f, 20f)]
	public float aimingDistance = 8f;

	[SerializeField]
	public Vector2 aimingOffset = Vector2.one;

	[SerializeField, Range(0f,100f)]
	public float focusRadius = 3f;

	[SerializeField, Range(0f, 1f)]
	public float focusCentering = 0.5f;

	[SerializeField, Range(1f, 360f)]
	public float rotationSpeed = 25f;

	[SerializeField, Range(-89f, 89f)]
	public float minVerticalAngle = -2f, maxVerticalAngle = 80f;

	[SerializeField, Range(0f,10f)]
	public float alignDelay = 1f;

	[SerializeField, Range(0f, 90f)]
	public float alignSmoothRange = 45f;

	[SerializeField]
	LayerMask obstructionMask = -1;

	[SerializeField]
	LayerMask aimLayerMask;

	[SerializeField]
	public Canvas aimingRecticle;

	[SerializeField]
	public bool rotationEnabled = false;

	public Vector3 focusOffset = Vector3.zero;

	public Camera regularCamera;

  Mouse mouse;

	Vector3 focusPoint, previousFocusPoint;

	public Vector2 orbitAngles = new Vector3(45f, 0f);

	float lastManualRotationTime;

	Vector3 CameraHalfExtends {
		get {
			Vector3 halfExtends;
			halfExtends.y =
				regularCamera.nearClipPlane *
				Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
			halfExtends.x = halfExtends.y * regularCamera.aspect;
			halfExtends.z = 0f;
			return halfExtends;
		}
	}

	public PlayerManager playerManager;


	void OnValidate ()
	{
		if (maxVerticalAngle < minVerticalAngle) {
			maxVerticalAngle = minVerticalAngle;
		}
	}

	public void Initialize(PlayerManager manager)
	{

		// regularCamera = transform.GetChild(0).gameObject.GetComponentInChildren<Camera>();
		focusPoint = focus.position;
		transform.localRotation = Quaternion.Euler(orbitAngles);
    mouse = Mouse.current;
		aimingRecticle.enabled = false;

		// playerManager = transform.parent.gameObject.GetComponent<PlayerManager>();
		playerManager = manager;
		initialized = true;
	}

	[Header("Random Testing Junk")]
	public float dofScaler = 1f;
	public float dofOffset = 0f;
	public float zoomScaler = 1f;
	void LateUpdate () {
		if (rotationEnabled && initialized)
		{
			UpdateAimTargetPoint();
			UpdateFocusPoint();
			Quaternion lookRotation;
			MassController ctrl = playerManager.gameObject.GetComponent<MassController>();
			Quaternion testRotation;
			if (ctrl != null)	testRotation = Quaternion.FromToRotation(Vector3.up, ctrl.localUp)*Quaternion.Euler(orbitAngles);
			else testRotation = Quaternion.identity;
			if (ManualRotation() || AutomaticRotation()) {
				ConstrainAngles();
				lookRotation = testRotation;
			}
			else {
				// lookRotation = transform.rotation;
				ConstrainAngles();
				lookRotation = testRotation;
			}

			Vector3 lookDirection = lookRotation * Vector3.forward;
			Vector3 lookPosition = focusPoint - lookDirection * distance;

			Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
			Vector3 rectPosition = lookPosition + rectOffset;
			Vector3 castFrom = focus.position;
			Vector3 castLine = rectPosition - castFrom;
			float castDistance = castLine.magnitude;
			Vector3 castDirection = castLine / castDistance;

			if (Physics.BoxCast(
				castFrom, CameraHalfExtends, castDirection, out RaycastHit hit,
				lookRotation, castDistance, obstructionMask
			)) {
				rectPosition = castFrom + castDirection * hit.distance;
				lookPosition = rectPosition - rectOffset;
			}

			transform.SetPositionAndRotation(lookPosition, lookRotation);

			HandleButtons();

			//-----------------------------------------
			// MassController ctrl = playerManager.gameObject.GetComponent<MassController>();
			if (ctrl != null)
			{
				Camera cam = GetComponentInChildren<Camera>();
				// cam.transform.rotation = Quaternion.LookRotation(transform.forward, ctrl.localUp);
			}
			//-----------------------------------------

			// ----------------------------------------
			// set up depth of field to track aim target
			// VolumeProfile profile = ppVolume.sharedProfile;
			// DepthOfField dph;
					// if (ppVolume)
					// {
					// 	if (ppVolume.sharedProfile.TryGetSettings<DepthOfField>(out DepthOfField dof))
					// 	{
					// 		dof.focusDistance.value = dofOffset + dofScaler*Vector3.Distance(transform.position, playerManager.aimTarget.position);
					// 		// dph.aperture.value = 30;
					// 		// dph.focalLength.value = blur_amt;
					// 		// dph.kernelSize.value = KernelSize.VeryLarge;
					// 	}
					// }
			// ----------------------------------------
		}

	}

	private float previousDistance = 0f;
	public void HandleButtons()
	{

    // do inventory if's
    if (playerManager.inputManager.inventoryOpen && !playerManager.inputManager.aiming)
    {
			previousDistance = distance;
      distance = inventoryDistance;
			aimingRecticle.enabled = false;
			focusOffset = Vector3.zero;
    }

		if (!playerManager.inputManager.inventoryOpen && playerManager.inputManager.aiming)
    {
			previousDistance = distance;
      distance = aimingDistance;
			aimingRecticle.enabled = true;
			focusOffset = aimingOffset.x*transform.right + aimingOffset.y*transform.up;
    }

		if (!playerManager.inputManager.inventoryOpen && !playerManager.inputManager.aiming && previousDistance != -1f)
    {
      distance = previousDistance;
			aimingRecticle.enabled = false;
			focusOffset = Vector3.zero;
			previousDistance = -1f;
    }

		// update distance based on scroll
		if (playerManager.inputManager.scrollPressed)
		{
			distance += zoomScaler*playerManager.inputManager.scrollValue.y;
			playerManager.inputManager.scrollPressed = false;
		}
		distance = Mathf.Min(Mathf.Max(distance , minMaxDistance.x), minMaxDistance.y);

  }

	public void UpdateAimTargetPoint () {
		RaycastHit hitInfo = new RaycastHit();
		float maxRange = 500f;
		if(Physics.Raycast (transform.position, transform.forward, out hitInfo, maxRange, aimLayerMask))
		{
			playerManager.aimTarget.position = transform.position + transform.forward*hitInfo.distance;
		}
		else
		{
			playerManager.aimTarget.position = transform.position + maxRange*transform.forward;
		}
	}

	public void UpdateFocusPoint () {
		previousFocusPoint = focusPoint;
		Vector3 targetPoint = focus.position + focusOffset;
		if (focusRadius > 0f) {
			float distance = Vector3.Distance(targetPoint, focusPoint);
			float t = 1f;
			if (distance > 0.01f && focusCentering > 0f) {
				t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
			}
			if (distance > focusRadius) {
				t = Mathf.Min(t, focusRadius / distance);
			}
			focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
		}
		else {
			focusPoint = targetPoint;
		}
	}

	public bool ManualRotation () {
		Vector2 input = new Vector2(
      -mouse.delta.y.ReadValue(),
      mouse.delta.x.ReadValue()
		);
				// map input by local up as well
				// MassController ctrl = playerManager.gameObject.GetComponent<MassController>();
				// float upAngle = Vector3.Angle(transform.up, ctrl.localUp);
				// float xVal = mouse.delta.x.ReadValue();
				// float yVal = mouse.delta.y.ReadValue();
				//
				// Vector2 input = new Vector2(
		    //   -yVal*Mathf.Cos(upAngle) + yVal*Mathf.Sin(upAngle),
		    //   xVal*Mathf.Cos(upAngle) + xVal*Mathf.Sin(upAngle)
				// );
		const float e = 0.001f;
		if (input.x < -e || input.x > e || input.y < -e || input.y > e) {
			orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
			lastManualRotationTime = Time.unscaledTime;
			return true;
		}
		return false;
	}

	public bool AutomaticRotation () {
		if (Time.unscaledTime - lastManualRotationTime < alignDelay) {
			return false;
		}

		Vector2 movement = new Vector2(
			focusPoint.x - previousFocusPoint.x,
			focusPoint.z - previousFocusPoint.z
		);
		float movementDeltaSqr = movement.sqrMagnitude;
		if (movementDeltaSqr < 0.0001f) {
			return false;
		}

		float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
		float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
		float rotationChange =
			rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
		if (deltaAbs < alignSmoothRange) {
			rotationChange *= deltaAbs / alignSmoothRange;
		}
		else if (180f - deltaAbs < alignSmoothRange) {
			rotationChange *= (180f - deltaAbs) / alignSmoothRange;
		}
		orbitAngles.y =
			Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
		return true;
	}

	public void ConstrainAngles () {
		orbitAngles.x =
			Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

		if (orbitAngles.y < 0f) {
			orbitAngles.y += 360f;
		}
		else if (orbitAngles.y >= 360f) {
			orbitAngles.y -= 360f;
		}
	}

	static float GetAngle (Vector2 direction) {
		float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
		return direction.x < 0f ? 360f - angle : angle;
	}

	void OnDisable ()
  {
		rotationEnabled = false;
  }

  void OnEnable ()
  {
		rotationEnabled = true;
  }

}
