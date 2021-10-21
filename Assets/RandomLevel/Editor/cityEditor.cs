using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CityStarGenerator))]
public class cityEditor : Editor
{
  CityStarGenerator city;
  Editor shapeEditor;

  public override void OnInspectorGUI()
  {
    using (var check = new EditorGUI.ChangeCheckScope())
    {
      base.OnInspectorGUI(); // do usual stuff
      if (check.changed && Application.isPlaying)
      {
        // regenerate level on change in inspector
        // city.GenerateBlocks();
        city.GenerateCity();
      }
    }
    DrawSettingsEditor(city.shapeSettings, city.OnShapeSettingsUpdated, ref shapeEditor);
    // normalize probability vectors
    float sumer = 0;
    float[] blockPrefabsProbs = new float[city.blockPrefabs.Length];
    foreach(BlockProb b in city.blockPrefabs)
    {
      sumer += b.prob;
    }
    foreach(BlockProb b in city.blockPrefabs)
    {
      b.prob = b.prob/sumer;
    }



    sumer = 0;
    float[] spherePrefabsProbs = new float[city.spherePrefabs.Length];
    foreach(BlockProb b in city.spherePrefabs)
    {
      sumer += b.prob;
    }
    foreach(BlockProb b in city.spherePrefabs)
    {
      b.prob = b.prob/sumer;
    }



    sumer = 0;
    float[] verticleAddOnsProbs = new float[city.verticleAddOns.Length];
    foreach(BlockProb b in city.verticleAddOns)
    {
      sumer += b.prob;
    }
    foreach(BlockProb b in city.verticleAddOns)
    {
      b.prob = b.prob/sumer;
    }



    sumer = 0;
    float[] horizontalAddOnsProbs = new float[city.horizontalAddOns.Length];
    foreach(BlockProb b in city.horizontalAddOns)
    {
      sumer += b.prob;
    }
    foreach(BlockProb b in city.horizontalAddOns)
    {
      b.prob = b.prob/sumer;
    }

  }

  void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref Editor editor)
  {
    if (settings != null)
    {
      using (var check = new EditorGUI.ChangeCheckScope())
      {

        CreateCachedEditor(settings, null, ref editor);
        editor.OnInspectorGUI();

        if (check.changed)
        {
          if (onSettingsUpdated != null)
          {
            onSettingsUpdated();
          }
        }
      }
    }
  }


  void OnEnable()
  {
    city = (CityStarGenerator)target;
  }
}
