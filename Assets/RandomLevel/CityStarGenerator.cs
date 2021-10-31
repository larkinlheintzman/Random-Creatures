using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.AI;

// [System.Serializable]
// public class BlockProb
// {
//     public Block blk;
//     [Range(0f,1f)]
//     public float prob;
// }

public class CityStarGenerator : MonoBehaviour
{

  [Header("Block Dimensions")]
  [SerializeField]
  public BlockProb[] blockPrefabs;
  [SerializeField]
  public BlockProb[] antiBlockPrefabs;
  [SerializeField]
  public BlockProb[] spherePrefabs;
  [SerializeField]
  public BlockProb[] verticleAddOns;
  [SerializeField]
  public BlockProb[] horizontalAddOns;
  [SerializeField]
  public int nodeNumber = 100; // how many start points are generated on the sphere
  [SerializeField]
  public float starRadius = 100f; // the base size of the thing
  [SerializeField]
  public float structureInset = 5f;
  [SerializeField]
  public float pixelScale = 2.0f;
  [SerializeField]
  public Vector3 blockSizeScale = new Vector3(1f,1f,1f);
  [SerializeField]
  public Vector3 blockSizeBase = new Vector3(1f,1f,1f);
  [SerializeField]
  public Vector3 blockRotationNoise = new Vector3(1f,1f,1f);
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
  public LayerMask planetLayerMask;
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

  [Header("NavMesh Params")]
  public NavMeshSurface surface;

  private RaycastHit raycastHit;
  private int gimmieFrames = 3;

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

  public int ProbPick(BlockProb[] blockArray)
  {
    // pick from listed blocks using probability weighting
    // float[] cumsum = new float[blockArray.Length];
    // cumsum[i] = sum;
    float sum = 0f;
    float randTest = Random.value; // test point
    for (int i = 0; i < blockArray.Length; i++)
    {
      sum += blockArray[i].prob;
      if (randTest < sum)
      {
        // picked this blk
        return i;
      }
    }
    return blockArray.Length - 1;

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

    Block[] newBlocks = new Block[(int)(nodeNumber)];
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

    // GenerateBlocks();
    // GenerateCity();
    // twiddle thumbs
    GenerateCity();

  }

  public float DistanceNoiseScaling(float val, float xPercent, float yPercent)
  {
    float xWeight = (1/Mathf.Pow(2f*Mathf.PI*distanceNoiseVariance, 0.5f))*Mathf.Exp(-Mathf.Pow(xPercent - 0.5f,2)/(2*distanceNoiseVariance));
    float yWeight = (1/Mathf.Pow(2f*Mathf.PI*distanceNoiseVariance, 0.5f))*Mathf.Exp(-Mathf.Pow(yPercent - 0.5f,2)/(2*distanceNoiseVariance));

    return val*(Mathf.Pow(xWeight*yWeight, distanceNoisePower));
  }

  public float pixelPhase = 1.0f;

  public void GenerateCity()
  {
    ResetBlocks();
    Vector3[] spherePoints = PointsOnSphere(nodeNumber);
    shapeGenerator = new ShapeGenerator(shapeSettings);

    // make planet
    int sInd = ProbPick(spherePrefabs);
    if (sInd != -1)
    {
      Block newEarth = Instantiate(spherePrefabs[sInd].blk, transform);
      newEarth.transform.localScale = starRadius*2*Vector3.one;
      planetTerrainGenerator planetMesher = newEarth.gameObject.GetComponentInChildren<planetTerrainGenerator>();
      if (planetMesher != null)
      {
        // print("meshing...");
        planetMesher.GenerateMesh();
      }
      // newEarth.Initialize(this, 0, starRadius*2*Vector3.one);
    }

    // place blocks where they need to go
    for (int i = 0; i < nodeNumber; i++)
    {
      // if (blk == null)
      // {
      //   return;
      // }
      Vector3 pointOnUnitSphere = spherePoints[i];
      // Vector3 pointInUnitPlane = new Vector3((float)i/(float)(gridDimensions.x - 1), 0.0f, (float)j/(float)(gridDimensions.y - 1));
      float noiseValue = shapeGenerator.CalculateNoise(pointOnUnitSphere);

      // do better sphere mapping later
      int xpix = (int)(Mathf.Abs(Mathf.Cos(pointOnUnitSphere.x*2f*Mathf.PI*pixelPhase))*pixelScale);
      int ypix = (int)(Mathf.Abs(Mathf.Sin(pointOnUnitSphere.y*2f*Mathf.PI*pixelPhase))*pixelScale);
      float generationVal = genTexture.GetPixel(xpix, ypix).grayscale;
      Color colorDir = dirTexture.GetPixel(xpix, ypix); // direction stored in red channel
      float dir = colorDir.r;
      Vector3 directionVal = new Vector3(Mathf.Cos(dir*Mathf.PI*2.0f), 0.0f, Mathf.Sin(dir*Mathf.PI*2.0f));

      if (generationVal < generationThreshold)
      {
        Block blk = Instantiate(blockPrefabs[ProbPick(blockPrefabs)].blk, transform);
        // blk.gameObject.SetActive(true);
        // rotate block to face towards dirval
        Vector3 tempDimensions = new Vector3(blockSizeBase.x + blockSizeScale.x*noiseValue, blockSizeBase.y + blockSizeScale.y*noiseValue, blockSizeBase.z + blockSizeScale.z*noiseValue);
        // up is outward from planet (DIRECTION IS DISABLED FOR NOW)
        // Vector3 targetDir = Vector3.forward;
        // face building randomly
        Vector3 targetDir = new Vector3(Random.value-0.5f, 0.0f, Random.value-0.5f).normalized;
        Vector3 forward = targetDir - pointOnUnitSphere * Vector3.Dot(targetDir, pointOnUnitSphere);

        blk.transform.rotation = Quaternion.LookRotation(forward, pointOnUnitSphere);
        blk.transform.position = (starRadius - structureInset)*pointOnUnitSphere + transform.position;
        StartCoroutine(FindBlockGround(blk, pointOnUnitSphere, forward, tempDimensions));
        // if (GetPlanetPoint(pointOnUnitSphere))
        // {
        //   blk.transform.rotation = Quaternion.LookRotation(forward, raycastHit.normal);
        //   blk.transform.position = raycastHit.point;
        // }
        // // blk.transform.localScale = tempDimensions;
      }
      else
      {
        Block blk = Instantiate(antiBlockPrefabs[ProbPick(antiBlockPrefabs)].blk, transform);
        // blk.gameObject.SetActive(true);
        // rotate block to face towards dirval
        Vector3 tempDimensions = new Vector3(blockSizeBase.x + blockSizeScale.x*noiseValue, blockSizeBase.y + blockSizeScale.y*noiseValue, blockSizeBase.z + blockSizeScale.z*noiseValue);
        // up is outward from planet (DIRECTION IS DISABLED FOR NOW)
        Vector3 targetDir = Vector3.forward;
        Vector3 forward = targetDir - pointOnUnitSphere * Vector3.Dot(targetDir, pointOnUnitSphere);

        blk.transform.rotation = Quaternion.LookRotation(forward, pointOnUnitSphere);
        blk.transform.position = (starRadius - structureInset)*pointOnUnitSphere + transform.position;
        if (GetPlanetPoint(pointOnUnitSphere))
        {
          blk.transform.rotation = Quaternion.LookRotation(forward, raycastHit.normal);
          blk.transform.position = raycastHit.point;
        }
        // blk.transform.localScale = tempDimensions;
        blk.Initialize(this, 0, tempDimensions);
      }
    }
  }

  public float planetTerrainCheckDistance = 100f;
  public bool GetPlanetPoint(Vector3 spherePoint)
  {
    // w raycasting lol
    raycastHit = new RaycastHit();
    Debug.DrawLine(transform.position + spherePoint*planetTerrainCheckDistance, transform.position + spherePoint*starRadius, Color.green, 0.1f);
    if (Physics.Raycast(transform.position + spherePoint*planetTerrainCheckDistance, -spherePoint, out raycastHit, starRadius + planetTerrainCheckDistance, planetLayerMask))
    {
      Debug.DrawLine(transform.position + spherePoint*planetTerrainCheckDistance, transform.position + spherePoint*starRadius, Color.blue, 1.1f);
      // print("found valid planet side point");
      return true;
    }
    return false;
  }

  public Vector3[] PointsOnSphere(int n)
  {
    List<Vector3> upts = new List<Vector3>();
    float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
    float off = 2.0f / n;
    float x = 0;
    float y = 0;
    float z = 0;
    float r = 0;
    float phi = 0;

    for (var k = 0; k < n; k++){
      y = k * off - 1 + (off /2);
      r = Mathf.Sqrt(1 - y * y);
      phi = k * inc;
      x = Mathf.Cos(phi) * r;
      z = Mathf.Sin(phi) * r;

      upts.Add(new Vector3(x, y, z));
    }
    Vector3[] pts = upts.ToArray();
    return pts;
  }

  public void OnShapeSettingsUpdated()
  {
    GenerateCity();
  }

  public IEnumerator FindBlockGround(Block blk, Vector3 spherePoint, Vector3 localForward, Vector3 dimensions)
  {
    bool groundFlag = false;
    for (int i = 0; i < gimmieFrames; i++)
    {
      // print("waiting on ground...");
      if (GetPlanetPoint(spherePoint) && !groundFlag)
      {
        print("found block ground spot!");
        blk.transform.rotation = Quaternion.LookRotation(localForward, raycastHit.normal);
        blk.transform.position = raycastHit.point;
        blk.Initialize(this, 0, dimensions);
        groundFlag = true;
      }
      yield return null;
    }
  }

}
