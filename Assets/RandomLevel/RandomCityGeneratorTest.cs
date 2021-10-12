using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;




public class RandomCityGeneratorTest : MonoBehaviour
{

  [Header("Block Dimensions")]
  [SerializeField]
  public Block[] blockPrefabs;
  [SerializeField]
  public BlockProb[] blockProbPrefabs;
  [SerializeField]
  public Block[] floorPrefabs;
  [SerializeField]
  public Block[] verticleAddOns;
  [SerializeField]
  public Block[] horizontalAddOns;
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
  public float childSizeScaler = 0.85f;

  [Header("City Behaviour")]
  [SerializeField]
  public float generationThreshold = 0.05f;
  [SerializeField]
  public float distanceNoisePower = 1f;
  [SerializeField]
  public float distanceNoiseVariance = 1f;
  [SerializeField]
  public LayerMask blockLayerMask;
  [SerializeField]
  public Block[] generatedBlocks;
  [SerializeField]
  public bool blocksGenerated = false;

  [Header("City Textures")]
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

  [Header("City Noise Settings")]
  [SerializeField]
  public ShapeGenerator shapeGenerator;
  [SerializeField]
  public ShapeSettings shapeSettings;

  [Header("Tree Params")]
  [SerializeField]
  public int maxDepth = 5;
  // [SerializeField]
  // public int maxDepth = 5;


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
      int numBlockPrefabs = blockPrefabs.Length;
      int counter = 0;
      for (int i = 0; i < gridDimensions.x; i++)
      {
        for (int j = 0; j < gridDimensions.y; j++)
        {

          newBlocks[counter] = Instantiate(blockPrefabs.PickOne(), transform);
          newBlocks[counter].transform.localPosition = Vector3.zero;

          // give block a floor, it'll clean it up
          Block newFloor = Instantiate(floorPrefabs.PickOne(), newBlocks[counter].transform);
          newFloor.transform.parent = null;
          newFloor.transform.rotation = Quaternion.identity;
          newFloor.transform.localScale = new Vector3(gridScale.x, 1.0f, gridScale.y);

          Vector2 gridOffset = new Vector3(gridDimensions.x/2f, gridDimensions.y/2f);
          newFloor.transform.position = new Vector3(gridScale.x*(i - gridOffset.x), 0.0f, gridScale.y*(j - gridOffset.y)) + transform.position;
          // please god pray for my hierarchy
          newFloor.transform.parent = transform;

          counter++;
        }
      }
      generatedBlocks = newBlocks;
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

    GenerateBlocks();
    GenerateCity();

  }

  public float DistanceNoiseScaling(float val, float xPercent, float yPercent)
  {
    float xWeight = (1/Mathf.Pow(2f*Mathf.PI*distanceNoiseVariance, 0.5f))*Mathf.Exp(-Mathf.Pow(xPercent - 0.5f,2)/(2*distanceNoiseVariance));
    float yWeight = (1/Mathf.Pow(2f*Mathf.PI*distanceNoiseVariance, 0.5f))*Mathf.Exp(-Mathf.Pow(yPercent - 0.5f,2)/(2*distanceNoiseVariance));

    return val*(Mathf.Pow(xWeight*yWeight, distanceNoisePower));
  }

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
          if (index >= generatedBlocks.Length)
          {
            return;
          }
          Block blk = generatedBlocks[index];
          if (blk == null)
          {
            return;
          }
          Vector3 pointInUnitPlane = new Vector3((float)i/(float)(gridDimensions.x - 1), 0.0f, (float)j/(float)(gridDimensions.y - 1));
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
            // rotate block to face towards dirval
            Vector3 tempDimensions = new Vector3(blockSizeBase.x + blockSizeScale.x*noiseValue, blockSizeBase.y + blockSizeScale.y*noiseValue, blockSizeBase.z + blockSizeScale.z*noiseValue);
            blk.transform.rotation = Quaternion.LookRotation(directionVal, blk.transform.up);
            blk.transform.position = new Vector3(gridScale.x*(i - gridOffset.x), 0.0f, gridScale.y*(j - gridOffset.y)) + transform.position;
            // blk.transform.localScale = tempDimensions;
                // blk.Initialize(this, 0, tempDimensions);

            //TODO this is where we block out the texture to inform other block placements
          }
          else
          {
            // Debug.Log("block " + index.ToString() + " did not make it");
            blk.gameObject.SetActive(false);
          }
        }
      }
    }
  }

  public void OnShapeSettingsUpdated()
  {
    GenerateCity();
  }

}
