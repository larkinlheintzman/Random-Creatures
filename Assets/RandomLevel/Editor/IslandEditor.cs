using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Island))]
public class IslandEditor : Editor
{
  Island island;

  Editor shapeEditor;
  Editor colorEditor;

  public override void OnInspectorGUI()
  {
    using (var check = new EditorGUI.ChangeCheckScope())
    {
      base.OnInspectorGUI();
      if (check.changed)
      {
        island.GenerateIsland();
      }
    }

    if (GUILayout.Button("Generate Island"))
    {
      island.GenerateIsland();
    }

    DrawSettingsEditor(island.shapeSettings, island.OnShapeSettingsUpdated, ref island.shapeSettingsFoldout, ref shapeEditor);
    DrawSettingsEditor(island.colorSettings, island.OnColorSettingsUpdated, ref island.colorSettingsFoldout, ref colorEditor);
  }

  void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
  {
    if (settings != null)
    {
      foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
      using (var check = new EditorGUI.ChangeCheckScope())
      {

        if (foldout == true)
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
  }

  private void OnEnable()
  {
    island = (Island)target;
  }
}
