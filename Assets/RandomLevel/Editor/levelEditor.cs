using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomLevelGenerator))]
public class levelEditor : Editor
{
  RandomLevelGenerator level;
  Editor shapeEditor;
  Editor colorEditor;

  public override void OnInspectorGUI()
  {
    using (var check = new EditorGUI.ChangeCheckScope())
    {
      base.OnInspectorGUI();
      if (check.changed)
      {
        level.GenerateLevel();
      }
    }

    if (GUILayout.Button("Generate Level"))
    {
      level.GenerateLevel();
    }

    if (GUILayout.Button("Reset Blocks"))
    {
      level.ResetBlocks();
    }

    DrawSettingsEditor(level.shapeSettings, level.OnShapeSettingsUpdated, ref level.shapeSettingsFoldout, ref shapeEditor);
    // DrawSettingsEditor(level.colorSettings, level.OnColorSettingsUpdated, ref level.colorSettingsFoldout, ref colorEditor);
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
    level = (RandomLevelGenerator)target;
  }
}
