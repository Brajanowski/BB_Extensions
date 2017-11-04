using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BB_Extensions.MaterialEditor {
  [CreateAssetMenu(fileName = "BetterEditor_Material", menuName = "Better Editor/New Material", order = 1)]
  public class MaterialData : ScriptableObject {
    [SerializeField]
    public List<Node> nodes = new List<Node>();

    [SerializeField]
    public List<Connection> connections = new List<Connection>();
  }

  [CustomEditor(typeof(MaterialData))]
  public class MaterialDataInspector : Editor {
    public override void OnInspectorGUI() {
      if (GUILayout.Button("Open in editor")) {
        MaterialEditor.EditMaterial((MaterialData)target);
      }
    }
  }
}