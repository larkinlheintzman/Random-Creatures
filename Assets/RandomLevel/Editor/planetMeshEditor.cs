using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(planetTerrainGenerator))]
public class planetMeshEditor : Editor
{
  public planetTerrainGenerator planet;
  public override void OnInspectorGUI()
  {
    using(var der = new EditorGUI.ChangeCheckScope())
    {
      base.OnInspectorGUI();
      if (der.changed)
      {
        if (planet.autoUpdate) planet.GenerateMesh();
      }
    }
  }

  public void OnEnable()
  {
    planet = (planetTerrainGenerator)target;
  }
}
