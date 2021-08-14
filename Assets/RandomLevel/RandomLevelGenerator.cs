using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomLevelGenerator : MonoBehaviour
{

  [SerializeField]
  public Block[] blockPrefabs;
  [SerializeField]
  public Vector3 gridDimensions = new Vector3(10, 10, 10);
  [SerializeField]
  public Vector3 gridStep = new Vector3(1,1,1);
  [SerializeField]
  public Vector3 blockSizeScale = new Vector3(1,1,1);
  [SerializeField]
  public Vector3 blockSizeBase = new Vector3(1,1,1);
  [SerializeField]
  public float generationThreshold = 0.05f;
  [SerializeField]
  public List<int> noiseLayerList;
  [SerializeField]
  public List<int> generationLayerList;
  [HideInInspector]
  [SerializeField]
  public Block[] generatedBlocks;
  [SerializeField]
  public ShapeGenerator shapeGenerator;
  [SerializeField]
  public ShapeSettings shapeSettings;
  [HideInInspector]
  [SerializeField]
  public bool shapeSettingsFoldout;
  [SerializeField]
  public bool autoUpdate = true;
  [SerializeField]
  public bool blocksGenerated = false;

  // void OnValidate()
  // {
  //   GenerateLevel();
  // }

  public void GenerateBlocks()
  {
    if (!blocksGenerated)
    {
      // generatedBlocks = new Block[(int)(gridDimensions.x*gridDimensions.y)];
      Block[] newBlocks = new Block[(int)(gridDimensions.x*gridDimensions.y*gridDimensions.z)];
      int numPrefabs = blockPrefabs.Length;
      for (int i = 0; i < gridDimensions.x*gridDimensions.y*gridDimensions.z; i++) {
        int prefabIndex = Random.Range(0, numPrefabs);
        newBlocks[i] = Instantiate(blockPrefabs[prefabIndex]).GetComponent<Block>();
        newBlocks[i].transform.parent = transform;
        newBlocks[i].transform.localPosition = Vector3.zero;
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

    Block[] newBlocks = new Block[(int)(gridDimensions.x*gridDimensions.y*gridDimensions.z)];
    generatedBlocks = newBlocks;
  }

  public void GenerateLevel()
  {
    if (blocksGenerated && gridDimensions.x*gridDimensions.y*gridDimensions.z == generatedBlocks.Length)
    {
      shapeGenerator = new ShapeGenerator(shapeSettings);
      Vector3 gridOffset = new Vector3(gridDimensions.x/2f, gridDimensions.y/2f, gridDimensions.z/2f);

      for (int i = 0; i < gridDimensions.x; i++) {
        for (int j = 0; j < gridDimensions.y; j++) {
          for (int k = 0; k < gridDimensions.z; k++) {
            int index = (int)(i*gridDimensions.y*gridDimensions.z + j*gridDimensions.z + k);
            // Debug.Log("linear index: " + index.ToString() + "/" + generatedBlocks.Length.ToString());
            if (index >= generatedBlocks.Length)
            {
              // Debug.Log("derped");
              // ResetBlocks();
              return;
            }
            Block blk = generatedBlocks[index];
            if (blk == null)
            {
              // ResetBlocks();
              return;
            }
            Vector3 pointInUnitCube = new Vector3((float)i/(float)(gridDimensions.x - 1), (float)j/(float)(gridDimensions.y - 1), (float)k/(float)(gridDimensions.z - 1));

            blk.transform.position = new Vector3(gridStep.x*i, gridStep.y*j, gridStep.z*k) + transform.position;
            // so perlin noise controls size still
            float noiseVal = shapeGenerator.CalculateLayerNoise(pointInUnitCube, noiseLayerList);
            float generationVal = shapeGenerator.CalculateLayerNoise(pointInUnitCube, generationLayerList);
            // add some lower bound so some blocks don't appear
            if (generationVal > generationThreshold)
            {
              blk.gameObject.SetActive(true);
              blk.dims = new Vector3(blockSizeBase.x + blockSizeScale.x*noiseVal, blockSizeBase.y + blockSizeScale.y*noiseVal, blockSizeBase.z + blockSizeScale.z*noiseVal);
              // borked
              // blk.Initialize();
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
    else
    {
      GenerateBlocks();
    }
  }

  public void OnShapeSettingsUpdated()
  {
    if (autoUpdate){
      GenerateLevel();
    }
  }
}
