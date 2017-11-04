using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BB_Extensions.BetterHierarchy {
  [InitializeOnLoad]
  public class BetterHierarchy {
    static Dictionary<string, Texture> icons = new Dictionary<string, Texture>() {
      { "Light", EditorGUIUtility.ObjectContent(null, typeof(Light)).image },
      { "MonoBehaviour", EditorGUIUtility.IconContent("cs Script Icon").image },
      { "ParticleSystem", EditorGUIUtility.ObjectContent(null, typeof(ParticleSystem)).image },
      { "Camera", EditorGUIUtility.IconContent("Camera Icon").image },
      { "MeshRenderer", EditorGUIUtility.ObjectContent(null, typeof(MeshRenderer)).image },
      { "Rigidbody", EditorGUIUtility.ObjectContent(null, typeof(Rigidbody)).image },
      { "Canvas", EditorGUIUtility.ObjectContent(null, typeof(Canvas)).image }
    };

    static int iconWidth = 18;
    static int iconHeight = 18;
    static int marginBetweenIcons = 18;

    static BetterHierarchy() {
      EditorApplication.hierarchyWindowItemOnGUI = _BetterHierarchy;
    }

    static void _BetterHierarchy(int instanceID, Rect selectedRect) {
      GameObject gameobject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

      if (gameobject != null) {
        Rect setActiveButtonRect = new Rect(selectedRect);
        setActiveButtonRect.x = setActiveButtonRect.width;
        setActiveButtonRect.width = iconWidth;
        setActiveButtonRect.height = iconHeight;
        gameobject.SetActive(GUI.Toggle(setActiveButtonRect, gameobject.activeSelf, ""));

        int iconCount = 0;
        foreach (KeyValuePair<string, Texture> bbIcon in icons) {
          iconCount += 1;
          if (gameobject.GetComponent(bbIcon.Key) != null) {
            DisplayIcon(bbIcon.Value, marginBetweenIcons * iconCount, selectedRect);
          }
        }
      }
    }

    static void DisplayIcon(Texture icon, int position, Rect selectedRect) {
      Rect gameobjectIconRect = new Rect();
      gameobjectIconRect.x = selectedRect.width - position;
      gameobjectIconRect.y = selectedRect.y;
      gameobjectIconRect.width = iconWidth;
      gameobjectIconRect.height = iconHeight;
      GUI.Label(gameobjectIconRect, icon);
    }
  }
}