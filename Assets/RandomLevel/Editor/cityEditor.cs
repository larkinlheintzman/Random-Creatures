using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomCityGenerator))]
public class cityEditor : Editor
{
  RandomCityGenerator city;
  Editor shapeEditor;

  public override void OnInspectorGUI()
  {
    using (var check = new EditorGUI.ChangeCheckScope())
    {
      base.OnInspectorGUI(); // do usual stuff
      if (check.changed)
      {
        // regenerate level on change in inspector
        city.GenerateCity();
      }
    }
    DrawSettingsEditor(city.shapeSettings, city.OnShapeSettingsUpdated, ref shapeEditor);
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
    city = (RandomCityGenerator)target;
  }
}
