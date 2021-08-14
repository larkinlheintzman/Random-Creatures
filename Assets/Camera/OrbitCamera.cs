using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour {

	[SerializeField]
	public RenderTexture pixelTexture;

	[SerializeField]
	public Transform focus = default;

	[SerializeField, Range(1f, 20f)]
	public float distance = 15f;

	[SerializeField, Range(1f, 20f)]
	public float defaultDistance = 15f;

	[SerializeField, Range(1f, 20f)]
	public float inventoryDistance = 10f;

	[SerializeField, Range(1f, 20f)]
	public float aimingDistance = 8f;

	[SerializeField]
	public Vector2 aimingOffset = Vector2.one;

	[SerializeField, Min(0f)]
	public float focusRadius = 3f;

	[SerializeField, Range(0f, 1f)]
	public float focusCentering = 0.5f;

	[SerializeField, Range(1f, 360f)]
	public float rotationSpeed = 25f;

	[SerializeField, Range(-89f, 89f)]
	public float minVerticalAngle = -2f, maxVerticalAngle = 80f;

	[SerializeField, Min(0f)]
	public float alignDelay = 1f;

	[SerializeField, Range(0f, 90f)]
	public float alignSmoothRange = 45f;

	[SerializeField]
	LayerMask obstructionMask = -1;

	[SerializeField]
	public Canvas aimingRecticle;

	[SerializeField]
	public bool rotationEnabled = false;

	public Transform aimTarget;

	public LayerMask aimLayerMask;

	public Vector3 focusOffset = Vector3.zero;

	Camera regularCamera;

  Mouse mouse;

	Vector3 focusPoint, previousFocusPoint;

	public Vector2 orbitAngles = new Vector2(45f, 0f);

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

	InputManager inputManager;

	void OnValidate () {
		if (maxVerticalAngle < minVerticalAngle) {
			maxVerticalAngle = minVerticalAngle;
		}
	}

	void Awake () {
		regularCamera = GetComponent<Camera>();
		focusPoint = focus.position;
		transform.localRotation = Quaternion.Euler(orbitAngles);
    mouse = Mouse.current;
		aimingRecticle.enabled = false;

		aimTarget = new GameObject().transform;
		aimTarget.gameObject.name = "AimTarget";

		inputManager = GameObject.Find("GameManager").GetComponent<InputManager>();
		// pixelTexture.height = 200;
		// pixelTexture.width = 200;
	}


	void LateUpdate () {
		if (rotationEnabled)
		{
			UpdateAimTargetPoint();
			UpdateFocusPoint();
			Quaternion lookRotation;
			if (ManualRotation() || AutomaticRotation()) {
				ConstrainAngles();
				lookRotation = Quaternion.Euler(orbitAngles);
			}
			else {
				lookRotation = transform.rotation;
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
		}
	}

	public void HandleButtons()
	{

    // do inventory if's
    if (inputManager.inventoryOpen && !inputManager.aiming)
    {
      distance = inventoryDistance;
			aimingRecticle.enabled = false;
			focusOffset = Vector3.zero;
    }

		if (!inputManager.inventoryOpen && inputManager.aiming)
    {
      distance = aimingDistance;
			aimingRecticle.enabled = true;
			focusOffset = aimingOffset.x*transform.right + aimingOffset.y*transform.up;
    }

		if (!inputManager.inventoryOpen && !inputManager.aiming)
    {
      distance = defaultDistance;
			aimingRecticle.enabled = false;
			focusOffset = Vector3.zero;
    }

  }

	void UpdateAimTargetPoint () {
		RaycastHit hitInfo = new RaycastHit();
		float maxRange = 500f;
		if(Physics.Raycast (transform.position, transform.forward, out hitInfo, maxRange, aimLayerMask))
		{
			aimTarget.position = transform.position + transform.forward*hitInfo.distance;
		}
		else
		{
			aimTarget.position = transform.position + maxRange*transform.forward;
		}
	}

	void UpdateFocusPoint () {
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

	bool ManualRotation () {
		Vector2 input = new Vector2(
      -mouse.delta.y.ReadValue(),
      mouse.delta.x.ReadValue()
		);
		const float e = 0.001f;
		if (input.x < -e || input.x > e || input.y < -e || input.y > e) {
			orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
			lastManualRotationTime = Time.unscaledTime;
			return true;
		}
		return false;
	}

	bool AutomaticRotation () {
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

	void ConstrainAngles () {
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
