using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BB_Extensions {

  [InitializeOnLoad]
  public class BB_BetterHierarchy {
    static Texture csScriptIcon;
    static Texture lightIcon;
    static Texture meshRendererIcon;
    static Texture cameraIcon;
    static Texture particleSystemIcon;

    static BB_BetterHierarchy() {
      EditorApplication.hierarchyWindowItemOnGUI = BetterHierarchy;

      csScriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image;
      lightIcon = EditorGUIUtility.ObjectContent(null, typeof(Light)).image;
      meshRendererIcon = EditorGUIUtility.ObjectContent(null, typeof(MeshRenderer)).image;
      cameraIcon = EditorGUIUtility.IconContent("Camera Icon").image;
      particleSystemIcon = EditorGUIUtility.ObjectContent(null, typeof(ParticleSystem)).image;   
    }

    static void BetterHierarchy(int instanceID, Rect selectionRect) {
      GameObject gameobject = EditorUtility.InstanceIDToObject (instanceID) as GameObject;

      if (gameobject != null) {
        Rect setActiveButtonRect = new Rect(selectionRect); 
        setActiveButtonRect.x = setActiveButtonRect.width;
        setActiveButtonRect.width = 18;
        setActiveButtonRect.height = 18;
        gameobject.SetActive(GUI.Toggle(setActiveButtonRect, gameobject.activeSelf, ""));

        if (gameobject.GetComponent<Light>() != null) {
          Rect gameobjectIconRect = new Rect();
          gameobjectIconRect.x = selectionRect.width - 24;
          gameobjectIconRect.y = selectionRect.y;
          gameobjectIconRect.width = 18;
          gameobjectIconRect.height = 18;
          GUI.Label(gameobjectIconRect, lightIcon);
        }

        if (gameobject.GetComponent<MonoBehaviour>() != null) {
          Rect gameobjectIconRect = new Rect();
          gameobjectIconRect.x = selectionRect.width - (24 * 2);
          gameobjectIconRect.y = selectionRect.y;
          gameobjectIconRect.width = 18;
          gameobjectIconRect.height = 18;
          GUI.Label(gameobjectIconRect, csScriptIcon);
        }

        if (gameobject.GetComponent<MeshRenderer>() != null) {
          Rect gameobjectIconRect = new Rect();
          gameobjectIconRect.x = selectionRect.width - (24 * 3);
          gameobjectIconRect.y = selectionRect.y;
          gameobjectIconRect.width = 18;
          gameobjectIconRect.height = 18;
          GUI.Label(gameobjectIconRect, meshRendererIcon);
        }

        if (gameobject.GetComponent<Camera>() != null) {
          Rect gameobjectIconRect = new Rect();
          gameobjectIconRect.x = selectionRect.width - (24 * 4);
          gameobjectIconRect.y = selectionRect.y;
          gameobjectIconRect.width = 18;
          gameobjectIconRect.height = 18;
          GUI.Label(gameobjectIconRect, cameraIcon);
        }

        if (gameobject.GetComponent<ParticleSystem>() != null) {
          Rect gameobjectIconRect = new Rect();
          gameobjectIconRect.x = selectionRect.width - (24 * 5);
          gameobjectIconRect.y = selectionRect.y;
          gameobjectIconRect.width = 18;
          gameobjectIconRect.height = 18;
          GUI.Label(gameobjectIconRect, particleSystemIcon);
        }
      }
    }
  }

}