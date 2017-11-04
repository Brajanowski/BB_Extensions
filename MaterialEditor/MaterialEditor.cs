using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BB_Extensions.MaterialEditor {
  public class MaterialEditor : EditorWindow {
    public static MaterialEditor materialEditorWindow;

    // core
    MaterialData materialData;

    // graph
    Vector2 mousePosition = Vector2.zero;
    Vector2 viewPosition = Vector2.zero;
    bool movingView = false;
    
    enum SelectionType {
      Nothing, Node, InputSlot, OutputSlot
    };
    SelectionType selectionType = SelectionType.Nothing;
    int selectedNode = -1;
    int selectedSlot = -1;

    bool makeConnectionMode = false;
    int startConnectionNode = -1;
    int startConnectionSlot = -1;

    // config/theme
    static public float gridSize = 32.0f;
    static public Color backgroundColor;
    static public Color nodeColor;
    static public Color outputNodeColor;
    static public Color gridColor;
    static public Color gridWiderColor;
    static public Color connectionColor;
    static public Color textColor;

    static public GUIStyle windowGuiStyle;
    static public GUIStyle nodeLabelStyle;
    static public GUIStyle nodeInputLabelStyle;
    static public GUIStyle nodeOutputLabelStyle;
    static public GUIStyle nodeFieldStyle;

    static Dictionary<Node.IOSlot.Type, Color> slotColors = new Dictionary<Node.IOSlot.Type, Color>();

    static void SetTheme() {
      backgroundColor = RGB255ToColor(24, 25, 29);
      nodeColor = RGB255ToColor(38, 39, 43);
      outputNodeColor = RGB255ToColor(132, 65, 10);
      gridColor = RGB255ToColor(19, 19, 20);
      gridWiderColor = RGB255ToColor(14, 14, 16);
      connectionColor = RGB255ToColor(234, 242, 19);
      textColor = RGB255ToColor(147, 147, 147);

      slotColors = new Dictionary<Node.IOSlot.Type, Color>();
      slotColors[Node.IOSlot.Type.Universal] = RGB255ToColor(96, 96, 183);
      slotColors[Node.IOSlot.Type.Float] = RGB255ToColor(193, 88, 88);
      slotColors[Node.IOSlot.Type.Vector2] = RGB255ToColor(44, 192, 193);
      slotColors[Node.IOSlot.Type.Vector3] = RGB255ToColor(93, 233, 88);
      slotColors[Node.IOSlot.Type.Color] = RGB255ToColor(245, 245, 245);

      windowGuiStyle = new GUIStyle();
      windowGuiStyle.fontSize = 12;
      windowGuiStyle.normal.background = EditorGUIUtility.whiteTexture;
      windowGuiStyle.normal.textColor = Color.white;
      windowGuiStyle.alignment = TextAnchor.UpperCenter;
      windowGuiStyle.fontStyle = FontStyle.Bold;
      windowGuiStyle.fontSize = 14;
      
      nodeLabelStyle = new GUIStyle();
      nodeLabelStyle.fontSize = 10;
      nodeLabelStyle.fontStyle = FontStyle.Bold;
      nodeLabelStyle.normal.textColor = Color.white;
      nodeLabelStyle.padding = new RectOffset(5, 5, 5, 5);

      nodeInputLabelStyle = new GUIStyle(nodeLabelStyle);
      nodeInputLabelStyle.alignment = TextAnchor.MiddleLeft;

      nodeOutputLabelStyle = new GUIStyle(nodeLabelStyle);
      nodeOutputLabelStyle.alignment = TextAnchor.MiddleRight;

      nodeFieldStyle = new GUIStyle(nodeLabelStyle);
      nodeFieldStyle.focused.textColor = Color.white;
      nodeFieldStyle.normal.textColor = Color.white;
      nodeFieldStyle.hover.textColor = Color.white;
      nodeFieldStyle.alignment = TextAnchor.MiddleRight;
    }

    [MenuItem("Window/Better Editor/Material Editor")]
    public static void ShowWindow() {
      materialEditorWindow = EditorWindow.GetWindow<MaterialEditor>("Material Editor");
      materialEditorWindow.Show();
    }

    public static void EditMaterial(MaterialData material) {
      ShowWindow();
      materialEditorWindow.materialData = material;
      EditorUtility.SetDirty(materialEditorWindow.materialData);
    }

    void OnGUI() {
      if (materialData == null) {
        EditorGUILayout.LabelField("Firstly choose a material file (You can create in \"Better Editor/New Material\"");
        Repaint();
        return;
      }

      // output must be here!!!
      if (materialData.nodes.Count == 0) {
        materialData.nodes.Add(new Node(Node.Type.Output));
      }

      SetTheme();

      // graph view
      GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));

      // background
      GUI.color = backgroundColor;
      GUI.DrawTexture(new Rect(0, 0, position.width, position.height), EditorGUIUtility.whiteTexture);

      // grid
      DrawGrid();

      // events
      Event ev = Event.current;
      mousePosition = ev.mousePosition;

      if (ev.type == EventType.mouseDown) {
        if (ev.button == 0) {
          if (makeConnectionMode) {
            makeConnectionMode = false;
            SelectByMousePosition();

            switch (selectionType) {
              case SelectionType.InputSlot: {
                TryConnect(startConnectionNode, selectedNode, startConnectionSlot, selectedSlot);
              } break;
            }
          } else {
            SelectByMousePosition();

            switch (selectionType) {
              case SelectionType.OutputSlot: {
                makeConnectionMode = true;
                startConnectionNode = selectedNode;
                startConnectionSlot = selectedSlot;
              } break;
            }
          }
        } else if (ev.button == 1) {
          if (makeConnectionMode) {
            makeConnectionMode = false;
          } else {
            SelectByMousePosition();

            switch (selectionType) {
              case SelectionType.Nothing: {
                GenericMenu menu = new GenericMenu();

                Node.Type[] nodeTypes = (Node.Type[])System.Enum.GetValues(typeof(Node.Type));

                for (int i = 2; i < nodeTypes.Length; ++i) {
                  menu.AddItem(new GUIContent("Create " + nodeTypes[i].ToString()), false, ContextCallbackCreateNode, nodeTypes[i]);
                }

                menu.ShowAsContext();
                ev.Use();
              } break;

              case SelectionType.Node: {
                if (materialData.nodes[selectedNode].type != Node.Type.Output) {
                  GenericMenu menu = new GenericMenu();
                  menu.AddItem(new GUIContent("Remove node"), false, ContextCallback, "remove node");
                  menu.ShowAsContext();
                }
              } break;

              case SelectionType.InputSlot: {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Clear connections"), false, ContextCallback, "remove connection input");
                menu.ShowAsContext();
              } break;

              case SelectionType.OutputSlot: {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Clear connections"), false, ContextCallback, "remove connection output");
                menu.ShowAsContext();
              } break;
            }
          }
        } else if (ev.button == 2) {
          movingView = true;
        }
      } else if (ev.type == EventType.MouseUp) {
        if (ev.button == 2) {
          movingView = false;
        }
      } else if (ev.type == EventType.MouseDrag) {
        if (movingView) {
          viewPosition += ev.delta;

          foreach (Node node in materialData.nodes) {
            node.rect.x += ev.delta.x;
            node.rect.y += ev.delta.y;
          }
        }
      }

      // draw connections
      foreach (Connection connection in materialData.connections) {
        Color color;
        if (!slotColors.TryGetValue(materialData.nodes[connection.nodeA].outputSlots[connection.nodeASlot].type, out color)) {
          color = connectionColor;
        }
        DrawConnection(materialData.nodes[connection.nodeA].GetOutputSlotRect(connection.nodeASlot), materialData.nodes[connection.nodeB].GetInputSlotRect(connection.nodeBSlot), color);
      }

      if (makeConnectionMode) {
        Color color;
        if (!slotColors.TryGetValue(materialData.nodes[startConnectionNode].outputSlots[startConnectionSlot].type, out color)) {
          color = connectionColor;
        }
        DrawConnection(materialData.nodes[startConnectionNode].GetOutputSlotRect(startConnectionSlot), new Rect(mousePosition.x, mousePosition.y, 1, 1), color);
      }

      // draw nodes
      BeginWindows();
      for (int idx = 0; idx < materialData.nodes.Count; ++idx) {
        Rect borderBox = materialData.nodes[idx].rect;
        borderBox.xMin -= 3.5f;
        borderBox.yMin -= 3.5f;
        borderBox.xMax += 3.5f;
        borderBox.yMax += 3.5f;
        GUI.color = gridColor;
        GUI.DrawTexture(borderBox, EditorGUIUtility.whiteTexture);

        GUI.color = nodeColor;
        GUI.contentColor = textColor;
        materialData.nodes[idx].rect = GUILayout.Window(idx, materialData.nodes[idx].rect, DrawNodeContent, materialData.nodes[idx].name, windowGuiStyle);

        // input and output slots
        for (int i = 0; i < materialData.nodes[idx].inputSlots.Count; ++i) {
          if (!materialData.nodes[idx].inputSlots[i].canBeUsed) {
            continue;
          }

          GUI.color = Color.white;

          Color slotRectColor;
          if (slotColors.TryGetValue(materialData.nodes[idx].inputSlots[i].type, out slotRectColor)) {
            GUI.color = slotRectColor;  
          }

          GUI.DrawTexture(materialData.nodes[idx].GetInputSlotRect(i), EditorGUIUtility.whiteTexture);
        }

        for (int i = 0; i < materialData.nodes[idx].outputSlots.Count; ++i) {
          if (!materialData.nodes[idx].outputSlots[i].canBeUsed) {
            continue;
          }

          GUI.color = Color.white;

          Color slotRectColor;
          if (slotColors.TryGetValue(materialData.nodes[idx].outputSlots[i].type, out slotRectColor)) {
            GUI.color = slotRectColor;  
          }

          GUI.DrawTexture(materialData.nodes[idx].GetOutputSlotRect(i), EditorGUIUtility.whiteTexture);
        }
      }
      EndWindows();

      // tools
      GUI.color = new Color(0, 0, 0, 0.82f);
      GUI.DrawTexture(new Rect(0, 0, position.width, 32), EditorGUIUtility.whiteTexture);

      // material name
      {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.alignment = TextAnchor.MiddleCenter;
        guiStyle.fontSize = 21;
        guiStyle.fontStyle = FontStyle.Bold;
        guiStyle.normal.textColor = Color.white;

        GUI.color = Color.white;
        GUI.contentColor = Color.white;
        GUI.Label(new Rect(0, 0, position.width, 32), materialData.name, guiStyle);
      }

      // buttons
      {
        if (GUI.Button(new Rect(5, 5, 22, 22), "C")) {
          MaterialEditor_Compiler.Compile(materialData);
        }

        if (GUI.Button(new Rect(5 + 27, 5, 22, 22), "V")) {
          ResetView();
        }

        if (GUI.Button(new Rect(5 + (27 * 2), 5, 22, 22), "R")) {
          materialData.nodes.Clear();
          materialData.connections.Clear();
        }
      }

      GUILayout.EndArea();

      Repaint();
    }

    void DrawGrid() {
      for (int y = 0; y < ((float)position.height / gridSize) + 1; ++y) {
        if (y % 4 != 0) {
          GUI.color = gridColor;
          GUI.DrawTexture(new Rect(0, y * gridSize, position.width, 1.5f), EditorGUIUtility.whiteTexture);
        }
      }

      for (int x = 0; x < ((float)position.width / gridSize) + 1; ++x) {
        if (x % 4 != 0) {
          GUI.color = gridColor;
          GUI.DrawTexture(new Rect(x * gridSize, 0, 1.5f, position.height), EditorGUIUtility.whiteTexture);
        }
      }

      for (int y = 0; y < ((float)position.height / gridSize) + 1; ++y) {
        if (y % 4 == 0) {
          GUI.color = gridWiderColor;
          GUI.DrawTexture(new Rect(0, y * gridSize, position.width, 3), EditorGUIUtility.whiteTexture);
        }
      }

      for (int x = 0; x < ((float)position.width / gridSize) + 1; ++x) {
        if (x % 4 == 0) {
          GUI.color = gridWiderColor;
          GUI.DrawTexture(new Rect(x * gridSize, 0, 3, position.height), EditorGUIUtility.whiteTexture);
        }
      }
    }

    void DrawConnection(Rect start, Rect end, Color color) {
      Vector3 startPosition = new Vector3(start.x + (start.width / 2.0f), start.y + (start.height / 2.0f), 0.0f);
      Vector3 endPosition = new Vector3(end.x + (end.width / 2.0f), end.y + (end.height / 2.0f), 0.0f);
      Vector3 startTangent = startPosition + Vector3.right * 50.0f;
      Vector3 endTangent = endPosition + Vector3.left * 50.0f;

      Handles.DrawBezier(startPosition, endPosition, startTangent, endTangent, color, null, 3.0f);
    }

    void DrawNodeContent(int id) {
      //Node node = materialData.nodes[id];
      materialData.nodes[id].ShowContent();
      GUI.DragWindow();
    }

    void ContextCallback(object obj) {
      string option = (string)obj;

      switch (option) {
        case "remove node": {
          RemoveNode(selectedNode);
        } break;

        case "remove connection input": {
          List<Connection> connectionsToRemove = new List<Connection>();

          foreach (Connection connection in materialData.connections) {
            if (connection.nodeB == selectedNode && connection.nodeBSlot == selectedSlot) {
              connectionsToRemove.Add(connection);
            }
          }

          foreach (Connection connection in connectionsToRemove) {
            materialData.connections.Remove(connection);
          }
        } break;

        case "remove connection output": {
          List<Connection> connectionsToRemove = new List<Connection>();

          foreach (Connection connection in materialData.connections) {
            if (connection.nodeA == selectedNode && connection.nodeASlot == selectedSlot) {
              connectionsToRemove.Add(connection);
            }
          }

          foreach (Connection connection in connectionsToRemove) {
            materialData.connections.Remove(connection);
          }
        } break;
      }

      Unselect();
    }

    void ContextCallbackCreateNode(object obj) {
      Node.Type nodeType = (Node.Type)obj;
      Node node = new Node(nodeType);
      node.rect.x = mousePosition.x;
      node.rect.y = mousePosition.y;
      materialData.nodes.Add(node);
    }

    void SelectByMousePosition() {
      for (int i = 0; i < materialData.nodes.Count; ++i) {
        if (materialData.nodes[i].rect.Contains(mousePosition)) {
          selectionType = SelectionType.Node;
          selectedNode = i;
          return;
        } else {
          for (int j = 0; j < materialData.nodes[i].inputSlots.Count; ++j) {
            if (materialData.nodes[i].inputSlots[j].canBeUsed && materialData.nodes[i].GetInputSlotRect(j).Contains(mousePosition)) {
              selectionType = SelectionType.InputSlot;
              selectedNode = i;
              selectedSlot = j;
              return;
            }
          }

          for (int j = 0; j < materialData.nodes[i].outputSlots.Count; ++j) {
            if (materialData.nodes[i].outputSlots[j].canBeUsed && materialData.nodes[i].GetOutputSlotRect(j).Contains(mousePosition)) {
              selectionType = SelectionType.OutputSlot;
              selectedNode = i;
              selectedSlot = j;
              return;
            }
          }
        }
      }

      selectionType = SelectionType.Nothing;
      selectedNode = -1;
    }

    void Unselect() {
      selectionType = SelectionType.Nothing;
      selectedNode = -1;
    }

    void RemoveNode(int nodeIndex) {
      List<Connection> connectionsToRemove = new List<Connection>();

      foreach (Connection connection in materialData.connections) {
        if (connection.nodeA == nodeIndex || connection.nodeB == nodeIndex) {
          connectionsToRemove.Add(connection);
        } else {
          if (connection.nodeA > nodeIndex)
            --connection.nodeA;
          if (connection.nodeB > nodeIndex)
            --connection.nodeB;
        }
      }

      foreach (Connection connection in connectionsToRemove) {
        materialData.connections.Remove(connection);
      }

      materialData.nodes.RemoveAt(nodeIndex);
    }

    bool IsInputSlotBusy(int inputNode, int inputSlot) {
      foreach (Connection connection in materialData.connections) {
        if (connection.nodeB == inputNode && connection.nodeBSlot == inputSlot) {
          return true;
        }
      }

      return false;
    }

    void TryConnect(int outputNode, int inputNode, int outputSlot, int inputSlot) {
      if (!materialData.nodes[outputNode].outputSlots[outputSlot].canBeUsed) {
        return;
      }

      if (!materialData.nodes[inputNode].inputSlots[inputSlot].canBeUsed) {
        return;
      }

      foreach (Connection connection in materialData.connections) {
        if (connection.nodeA == outputNode && connection.nodeB == inputNode) {
          if (connection.nodeBSlot == inputSlot) {
            return;
          }
        }
      }

      if (IsInputSlotBusy(inputNode, inputSlot)) {
        return;
      }

      if ((materialData.nodes[outputNode].outputSlots[outputSlot].type == materialData.nodes[inputNode].inputSlots[inputSlot].type) ||
          (materialData.nodes[outputNode].outputSlots[outputSlot].type == Node.IOSlot.Type.Universal ||
           materialData.nodes[inputNode].inputSlots[inputSlot].type == Node.IOSlot.Type.Universal)) {
        materialData.connections.Add(new Connection(outputNode, inputNode, outputSlot, inputSlot));
      }
    }

    void ResetView() {
      foreach (Node node in materialData.nodes) {
        node.rect.x -= viewPosition.x;
        node.rect.y -= viewPosition.y;
      }

      viewPosition = Vector2.zero;
    }

    static Color RGB255ToColor(int r, int g, int b) {
      return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f);
    }
  }
}