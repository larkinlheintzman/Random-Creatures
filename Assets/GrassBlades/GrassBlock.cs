using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProceduralGrassRenderer))]
public class GrassBlock : Block
{

  public int resolution = 100;
  public float size = 10.0f;
  public float randomScaler = 2f;
  public float raycastDistance = 10f;
  public float raycastOffset = 5f;
  [Range(-1,1)]
  public int raycastDirection = 1;
  public float groundOffset = 2f;
  public ProceduralGrassRenderer grassRenderer;
  public MeshFilter meshFilter;
  public Mesh mesh;
  public LayerMask grassMask;
  public bool grassChecked = false;

  public override void Initialize(CityStarGenerator generator, int genDepth, Vector3 localScale)
  {
    base.Initialize(generator, genDepth, localScale);
    transform.position += groundOffset*transform.up;
    GenerateMesh();
    grassChecked = false;

  }

  // account for changing terrain/my dumb ass planet builder
  public int gimmieFrames = 10;
  public int gimmieCounter = 0;
  public void LateUpdate()
  {
    // first FIRST frame of update
    if (gimmieCounter > gimmieFrames && !grassChecked)
    {
      GenerateMesh();
      grassChecked = true;
      gimmieCounter = 0;
    }
    else if (!grassChecked)
    {
      gimmieCounter += 1;
    }
  }

  public void GenerateMesh()
  {
    // Debug.Log("ye haw n such");
    Vector3[] vertices = new Vector3[resolution * resolution];
    int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
    int triIndex = 0;
    meshFilter.sharedMesh = new Mesh();
    mesh = meshFilter.sharedMesh;
    grassRenderer.sourceMesh = meshFilter.sharedMesh;

    for (int y = 0; y < resolution; y++)
    {
      for (int x = 0; x < resolution; x++)
      {
        int i = x + y*resolution;
        Vector2 percent = new Vector2(x,y)/(resolution-1);
        Vector3 flatPos = new Vector3(size*(percent.x - 0.5f) + randomScaler*(Random.value - 0.5f), 0.0f, size*(percent.y - 0.5f) + randomScaler*(Random.value - 0.5f));
        // float xpos = size*(percent.x - 0.5f) + randomScaler*(Random.value - 0.5f);
        // float ypos = size*(percent.y - 0.5f) + randomScaler*(Random.value - 0.5f);
        Debug.DrawLine(transform.position + transform.rotation*flatPos, transform.position + transform.rotation*flatPos + raycastDirection*raycastDistance*transform.up, Color.green, 0.5f);
        if (Physics.Raycast(transform.position + transform.rotation*(flatPos + raycastOffset*Vector3.up), raycastDirection*transform.up, out RaycastHit heet, raycastDistance, grassMask))
        {
          Debug.DrawLine(transform.position + transform.rotation*flatPos, transform.position + transform.rotation*flatPos + raycastDirection*raycastDistance*transform.up, Color.red, 1f);
          vertices[i] = flatPos + new Vector3(0.0f, raycastDirection*(heet.distance-raycastOffset), 0.0f);
        }
        else
        {
          // vertices[i] = new Vector3(size*(percent.x - 0.5f) + randomScaler*(Random.value - 0.5f), 0.0f, size*(percent.y - 0.5f) + randomScaler*(Random.value - 0.5f));
          vertices[i] = flatPos;
        }

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
    mesh.Clear();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
    // meshFilter.mesh = mesh;

    ProceduralGrassRenderer rdr = gameObject.GetComponent<ProceduralGrassRenderer>();
    if (rdr != null) rdr.OnEnable();
  }
}
