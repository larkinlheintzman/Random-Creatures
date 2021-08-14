using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixCamera : MonoBehaviour
{
    [HideInInspector]
    public RenderTexture pixelRT;
    [HideInInspector]
    public Camera pixelCamera;
    [HideInInspector]
    public Camera mainCamera;
    [HideInInspector]
    public Canvas pixelCanvas; // thing on which to project
    [HideInInspector]
    public RawImage pixelImage;

    public LayerMask mainCameraSees;
    public LayerMask pixelCameraSees;

    int rtWidth = 225;
    int rtHeight = 225;

    void Start()
    {
      mainCamera = GetComponent<Camera>();
      mainCamera.cullingMask = mainCameraSees;
      pixelRT = new RenderTexture(rtHeight, rtWidth, 16, RenderTextureFormat.ARGB32);
      pixelRT.filterMode = FilterMode.Point;
      pixelRT.Create();

      pixelCanvas = new Canvas();
      GameObject canvasContainer = new GameObject();
      canvasContainer.name = "pixelCanvas";
      canvasContainer.transform.parent = transform;
      canvasContainer.layer = 1 << 0; // transparent fx layer

      canvasContainer.AddComponent<Canvas>();
      pixelCanvas = canvasContainer.GetComponent<Canvas>();
      pixelCanvas.renderMode = RenderMode.ScreenSpaceCamera;
      pixelCanvas.worldCamera = mainCamera;
      pixelCanvas.planeDistance = 1.0f;

      canvasContainer.AddComponent<Canvas>();
      canvasContainer.AddComponent<CanvasScaler>();
      canvasContainer.AddComponent<GraphicRaycaster>();

      // pixelCamera = Instantiate(mainCamera).GetComponent<Camera>();
      GameObject pixelCameraContainer = new GameObject();
      pixelCameraContainer.name = "pixelCamera";
      pixelCameraContainer.AddComponent<Camera>();
      pixelCamera = pixelCameraContainer.GetComponent<Camera>();
      pixelCamera.CopyFrom(mainCamera);
      pixelCameraContainer.transform.parent = transform;
      pixelCamera.targetTexture = pixelRT;
      pixelCamera.cullingMask = pixelCameraSees;

      // set low res texture to raw image to put on canvas
      GameObject pixelImageContainer = new GameObject();
      pixelImageContainer.transform.parent = canvasContainer.transform;
      pixelImageContainer.name = "pixelImage";
      pixelImageContainer.AddComponent<RawImage>();
      pixelImage = pixelImageContainer.GetComponent<RawImage>();
      pixelImage.texture = pixelRT;

      // mess with rect transform
      RectTransform imgRect = pixelImageContainer.GetComponent<RectTransform>();
      Vector2 rectMid = new Vector2(0.5f, 0.5f);
      float heightRatio = 1f;
      float widthRatio = 1f;

      imgRect.sizeDelta = Vector2.zero;
      imgRect.anchoredPosition = Vector2.zero;
      imgRect.anchoredPosition3D = Vector3.zero;
      imgRect.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
      imgRect.localScale = Vector3.one;

      imgRect.anchorMin = new Vector2(rectMid.x - widthRatio / 2,
                                  rectMid.y - heightRatio / 2);
      imgRect.anchorMax = new Vector2(rectMid.x + widthRatio / 2,
                                  rectMid.y + heightRatio / 2);
    }

    // void Start()
    // {
    // }

    // void Update()
    // {
    //
    // }
}
