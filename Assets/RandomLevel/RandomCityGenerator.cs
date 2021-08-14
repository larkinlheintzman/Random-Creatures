using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RandomCityGenerator : MonoBehaviour
{

  [SerializeField]
  public Block[] blockPrefabs;
  [SerializeField]
  public Block[] floorPrefabs;
  [SerializeField]
  public BlockAddOn[] addOnPrefabs;
  [SerializeField]
  public SideWalkAddOn[] sideWalkPrefabs;
  [SerializeField]
  public Vector2 gridDimensions = new Vector2(10f, 10f);
  [SerializeField]
  public Vector2 gridScale = new Vector3(1f,1f);
  [SerializeField]
  public Vector2 gridPixelScale = new Vector3(1f,1f);
  [SerializeField]
  public Vector3 blockSizeScale = new Vector3(1f,1f,1f);
  [SerializeField]
  public Vector3 blockSizeBase = new Vector3(1f,1f,1f);
  [SerializeField]
  public Vector2 floorIncrements = new Vector2(10f, 10f);
  [SerializeField]
  public float generationThreshold = 0.05f;
  [SerializeField, Range(0.1f, 5f)]
  public float sideWalkThickness = 2.0f; // as in curb height
  [SerializeField]
  public float sideWalkWidth = 5.0f; // as in from wall of building
  [SerializeField]
  public float addOnCheckDistance = 3.0f;
  [SerializeField]
  public float distanceNoisePower = 1f;
  [SerializeField]
  public float distanceNoiseVariance = 1f;
  [SerializeField]
  public LayerMask blockLayerMask;
  [SerializeField]
  [HideInInspector]
  public Block[] generatedBlocks;
  [SerializeField]
  [HideInInspector]
  public Block[] generatedFloors;
  [SerializeField]
  public bool blocksGenerated = false;
  [SerializeField]
  public RenderTexture cityTexture;
  [SerializeField]
  public RenderTexture cityDirectionTexture;
  [SerializeField]
  public cityGridRunner cityGrid;
  [SerializeField]
  public Texture2D genTexture;
  [SerializeField]
  public Texture2D dirTexture;
  [SerializeField]
  public ShapeGenerator shapeGenerator;
  [SerializeField]
  public ShapeSettings shapeSettings;

  // void OnValidate()
  // {
  //   GenerateLevel();
  // }

  WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

  public void SaveTextureToImage(RenderTexture rt, string filename)
  {
      // yield return frameEnd;

      Texture2D virtualPhoto = new Texture2D(cityGrid.size,cityGrid.size, TextureFormat.RGB24, false);
      RenderTexture.active = rt;
      // false, meaning no need for mipmaps

      virtualPhoto.ReadPixels( new Rect(0, 0, cityGrid.size,cityGrid.size), 0, 0);
      virtualPhoto.Apply();

      RenderTexture.active = null; //can help avoid errors
      // consider ... Destroy(rt);

      byte[] bytes;
      bytes = virtualPhoto.EncodeToPNG();

      File.WriteAllBytes(GetFileName(filename), bytes);
  }

  private string GetFileName(string rawName)
  {
    string r = Application.persistentDataPath + "/" + rawName + ".png";
    return r;
  }

  public void GenerateBlocks()
  {
    if (!blocksGenerated)
    {
      // generate block where white is not
      Block[] newBlocks = new Block[(int)(gridDimensions.x*gridDimensions.y)];
      // could be weird if floors dont evenly divide but whatever
      Block[] newFloors = new Block[(int)((gridDimensions.x/floorIncrements.x)*(gridDimensions.y/floorIncrements.y))];
      int numBlockPrefabs = blockPrefabs.Length;
      int numFloorPrefabs = floorPrefabs.Length;
      for (int i = 0; i < gridDimensions.x*gridDimensions.y; i++) {
        newBlocks[i] = Instantiate(blockPrefabs.PickOne(), transform);
        newBlocks[i].transform.localPosition = Vector3.zero;
      }
      for (int i = 0; i < (gridDimensions.x/floorIncrements.x)*(gridDimensions.y/floorIncrements.y); i++) {
        newFloors[i] = Instantiate(floorPrefabs.PickOne(), transform);
        newFloors[i].transform.localPosition = Vector3.zero;
      }
      generatedBlocks = newBlocks;
      generatedFloors = newFloors;
      blocksGenerated = true;
    }
    else
    {
      ResetBlocks();
      GenerateBlocks();
    }
  }

  public void ResetBlocks()
  {
    blocksGenerated = false;
    while(transform.childCount > 0)
    {
      foreach (Transform child in transform)
      {
        GameObject.DestroyImmediate(child.gameObject);
      }
    }

    Block[] newBlocks = new Block[(int)(gridDimensions.x*gridDimensions.y)];
    generatedBlocks = newBlocks;
    Block[] newFloors = new Block[(int)(((float)gridDimensions.x/floorIncrements.x)*((float)gridDimensions.y/floorIncrements.y))];
    generatedFloors = newFloors;
  }

  public void Start()
  {
    // find render texture
    cityGrid = GetComponent<cityGridRunner>();
    cityGrid.RunCityGrid();
    cityTexture = cityGrid.renderTexture;
    cityDirectionTexture = cityGrid.renderTextureRotation;

    // Debug.Log(GetFileName());
    SaveTextureToImage(cityTexture, "gridTexture");
    SaveTextureToImage(cityDirectionTexture, "gridDirectionTexture");

    genTexture = new Texture2D(cityGrid.size,cityGrid.size, TextureFormat.RGB24, false);
    dirTexture = new Texture2D(cityGrid.size,cityGrid.size, TextureFormat.RGB24, false);
    // load image as 2D texture
    if (File.Exists(GetFileName("gridTexture")))
    {
      byte[] imgBytes = File.ReadAllBytes(GetFileName("gridTexture"));
      genTexture.LoadImage(imgBytes);
    }
    // same for dir texture
    if (File.Exists(GetFileName("gridDirectionTexture")))
    {
      byte[] imgBytes = File.ReadAllBytes(GetFileName("gridDirectionTexture"));
      dirTexture.LoadImage(imgBytes);
    }

    // sort add ons
    // List<SideWalkAddOn> sideWalkList = new List<SideWalkAddOn>();
    // List<BlockAddOn> addOnList = new List<BlockAddOn>();
    // foreach(BlockAddOn addOn in addOnPrefabs)
    // {
    //   if (addOn is SideWalkAddOn)
    //   {
    //     sideWalkList.Add((SideWalkAddOn)addOn);
    //   }
    //   else
    //   {
    //     addOnList.Add(addOn);
    //   }
    // }

    GenerateBlocks();
    GenerateCity();

  }

  public float DistanceNoiseScaling(float val, float xPercent, float yPercent)
  {
    float xWeight = (1/Mathf.Pow(2f*Mathf.PI*distanceNoiseVariance, 0.5f))*Mathf.Exp(-Mathf.Pow(xPercent - 0.5f,2)/(2*distanceNoiseVariance));
    float yWeight = (1/Mathf.Pow(2f*Mathf.PI*distanceNoiseVariance, 0.5f))*Mathf.Exp(-Mathf.Pow(yPercent - 0.5f,2)/(2*distanceNoiseVariance));

    return val*(Mathf.Pow(xWeight*yWeight, distanceNoisePower));
  }

  // public float GaussianDist(float )
  // {
  //
  // }

  public void GenerateCity()
  {
    if (blocksGenerated && gridDimensions.x*gridDimensions.y == generatedBlocks.Length)
    {
      shapeGenerator = new ShapeGenerator(shapeSettings);
      Vector2 gridOffset = new Vector3(gridDimensions.x/2f, gridDimensions.y/2f);

      // place blocks where they need to go
      for (int i = 0; i < gridDimensions.x; i++)
      {
        for (int j = 0; j < gridDimensions.y; j++)
        {
          int index = (int)(i*gridDimensions.x + j);
          // Debug.Log("linear index: " + index.ToString() + "/" + generatedBlocks.Length.ToString());
          if (index >= generatedBlocks.Length)
          {
            return;
          }
          Block blk = generatedBlocks[index];
          if (blk == null)
          {
            return;
          }
          Vector3 pointInUnitPlane = new Vector3((float)i/(float)(gridDimensions.x - 1), (float)j/(float)(gridDimensions.y - 1));
          float noiseValue = shapeGenerator.CalculateNoise(pointInUnitPlane);
          noiseValue = DistanceNoiseScaling(noiseValue, i/gridDimensions.x, j/gridDimensions.y);
          int xpix = (int)(i*(cityGrid.size/gridDimensions.x)*gridPixelScale.x);
          int ypix = (int)(j*(cityGrid.size/gridDimensions.y)*gridPixelScale.y);

          float generationVal = genTexture.GetPixel(xpix, ypix).grayscale;
          Color colorDir = dirTexture.GetPixel(xpix, ypix); // direction stored in red channel
          float dir = colorDir.r;
          Vector3 directionVal = new Vector3(Mathf.Cos(dir*Mathf.PI*2.0f), 0.0f, Mathf.Sin(dir*Mathf.PI*2.0f));

          if (generationVal < generationThreshold)
          {
            blk.gameObject.SetActive(true);
            Vector3 tempDimensions = new Vector3(blockSizeBase.x + blockSizeScale.x*noiseValue, blockSizeBase.y + blockSizeScale.y*noiseValue, blockSizeBase.z + blockSizeScale.z*noiseValue);
            // rotate block to face towards dirval
            blk.transform.rotation = Quaternion.LookRotation(directionVal, blk.transform.up);
            blk.transform.position = new Vector3(gridScale.x*(i - gridOffset.x), tempDimensions.y/2f, gridScale.y*(j - gridOffset.y)) + transform.position;
            blk.dims = tempDimensions;
            blk.Initialize(this);
          }
          else
          {
            // Debug.Log("block " + index.ToString() + " did not make it");
            blk.gameObject.SetActive(false);
          }
        }
      }

      // place floors where they need to go
      for (int i = 0; i < ((float)gridDimensions.x/floorIncrements.x); i++) {
        for (int j = 0; j < ((float)gridDimensions.y/floorIncrements.y); j++) {
          int index = (int)(i*((float)gridDimensions.x/floorIncrements.x) + j);
          // Debug.Log("linear index: " + index.ToString() + "/" + generatedBlocks.Length.ToString());
          if (index >= generatedFloors.Length)
          {
            Debug.Log("floor derped");
            // ResetBlocks();
            return;
          }
          Block flr = generatedFloors[index];
          if (flr == null)
          {
            // ResetBlocks();
            return;
          }
          flr.transform.position = new Vector3(gridScale.x*(i*floorIncrements.x - gridOffset.x + floorIncrements.x/2f) - blockSizeBase.x, -0.5f, gridScale.y*(j*floorIncrements.y - gridOffset.y + floorIncrements.y/2f) - blockSizeBase.z) + transform.position;

          // float generationVal = Mathf.Infinity;
          flr.gameObject.SetActive(true);
          flr.dims = new Vector3(gridScale.x*(floorIncrements.x),1f,gridScale.y*(floorIncrements.y));
          flr.Initialize(this);
        }
      }
    }

    // go back through and initialize
    // for (int i = 0; i < generatedBlocks.Length; i++) {
    //   Block blk = generatedBlocks[i];
    //   blk.Initialize(this);
    // }
    // for (int i = 0; i < generatedFloors.Length; i++) {
    //   Block flr = generatedFloors[i];
    //   flr.Initialize(this);
    // }
  }

  public void OnShapeSettingsUpdated()
  {
    GenerateCity();
  }

}
