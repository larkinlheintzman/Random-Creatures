using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planetTerrainGenerator : MonoBehaviour
{

  public MeshRenderer[] faceRenderers = new MeshRenderer[6];
  public MeshFilter[] faceFilters = new MeshFilter[6];
  public MeshCollider[] faceColliders = new MeshCollider[6];
  public Mesh[] faceMeshs = new Mesh[6];
  public Vector3[] localUps = new Vector3[6] {Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back};
  public Material planetMaterial;
  public int resolution = 100;
  public float size = 10f;
  public bool meshMade = false;
  public bool autoUpdate = false;

  [Header("Mesh Wobbles:")]
  public ShapeGenerator shapeGenerator;
  public ShapeSettings shapeSettings;

    // Start is called before the first frame update
    // void Start()
    // {
    //   GenerateMesh();
    // }
    public void Update()
    {
      if (!meshMade) GenerateMesh();
    }

    public void GenerateMesh()
    {

      shapeGenerator = new ShapeGenerator(shapeSettings);
      int maxCounter = 1000;
      int currentCount = 0;
      while(transform.childCount != 0)
      {
        foreach(Transform ch in transform)
        {
          DestroyImmediate(ch.gameObject);
        }
        currentCount += 1;
        if (currentCount > maxCounter) break;
      }

      for (int f = 0; f < 6; f++)
      {
        GameObject faceObj = new GameObject($"face_{f}");
        faceObj.layer = gameObject.layer;
        faceObj.transform.parent = transform;
        faceFilters[f] = faceObj.AddComponent<MeshFilter>();
        faceFilters[f].sharedMesh = new Mesh();
        faceFilters[f].sharedMesh = new Mesh();
        faceMeshs[f] = faceFilters[f].sharedMesh;
        faceColliders[f] = faceObj.AddComponent<MeshCollider>();
        faceColliders[f].sharedMesh = faceFilters[f].sharedMesh;
        // faceColliders[f].convex = true;
        faceColliders[f].convex = false;

        Vector3[] vertices = new Vector3[resolution * resolution];
        Vector2[] uvs = new Vector2[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        // deploy absolutely god brain moves from seb once again
        Vector3 axisA = new Vector3(localUps[f].y, localUps[f].z, localUps[f].x);
        Vector3 axisB = Vector3.Cross(localUps[f], axisA);

        for (int y = 0; y < resolution; y++)
        {
          for (int x = 0; x < resolution; x++)
          {
            int i = x + y*resolution;
            Vector2 percent = new Vector2(x,y)/(resolution-1);
            Vector3 pointOnUnitCube = localUps[f] + (percent.x - 0.5f)*2*axisA + (percent.y - 0.5f)*2*axisB;
            Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
            // vertices[i] = new Vector3(size*(percent.x - 0.5f) + randomScaler*(Random.value - 0.5f), 0.0f, size*(percent.y - 0.5f) + randomScaler*(Random.value - 0.5f));
            uvs[i] = percent;
            vertices[i] = size/2f*shapeGenerator.CalculatePointOnPlanet(pointOnUnitSphere);

            if (x != resolution - 1 && y != resolution - 1)
            {
              triangles[triIndex] = i;
              triangles[triIndex+1] = i + resolution + 1;
              triangles[triIndex+2] = i + resolution;

              triangles[triIndex+3] = i;
              triangles[triIndex+4] = i + 1;
              triangles[triIndex+5] = i + resolution + 1;
              triIndex += 6;
            }
          }
        }
        // uvs[0] = new Vector2(0,1); //top-left
        // uvs[1] = new Vector2(1,1); //top-right
        // uvs[2] = new Vector2(0,0); //bottom-left
        // uvs[3] = new Vector2(1,0); //bottom-right


        faceMeshs[f].Clear();
        faceMeshs[f].vertices = vertices;
        faceMeshs[f].triangles = triangles;
        faceMeshs[f].uv = uvs;
        faceMeshs[f].RecalculateNormals();

        faceRenderers[f] = faceObj.AddComponent<MeshRenderer>();
        faceRenderers[f].sharedMaterial = planetMaterial;
        faceObj.transform.localScale = Vector3.one;
        faceObj.transform.localPosition = Vector3.zero;
        faceObj.transform.rotation = Quaternion.identity;


                    // grassGenerator = faceObj.AddComponent<MeshFilter>();
      }
      meshMade = true;
    }

}
