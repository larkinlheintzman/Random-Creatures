using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(atmosphereRunner))]
public class atmosphereEditor : Editor
{
  public atmosphereRunner atmo;
  public override void OnInspectorGUI()
  {
    using(var derp = new EditorGUI.ChangeCheckScope())
    {
      base.OnInspectorGUI();
      if (derp.changed)
      {
        atmo.SetValues();
      }
    }
  }

  public void OnEnable()
  {
    atmo = (atmosphereRunner)target;
  }
}
