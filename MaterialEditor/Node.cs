using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BB_Extensions.MaterialEditor {
  [System.Serializable]
  public class Node {
    public enum Type {
      Unknown,
      Output,
      ConstFloat,
      ConstVector2,
      ConstVector3,
      ConstColor,
      Texture,
      Time,
      SinTime,
      CosTime,
      Add,
      Subtraction,
      Multiply,
      Divide,
      PackVector2,
      PackVector3,
      PackColor,
      Lerp
    }

    [SerializeField]
    public Type type;

    [SerializeField]
    public string name = "Base";

    [SerializeField]
    public Rect rect = new Rect(0, 0, 100, 100);

    [System.Serializable]
    public class IOSlot {
      [SerializeField]
      public string label;

      public enum Type {
        Universal, Float, Vector2, Vector3, Color, Texture
      }
      [SerializeField]
      public Type type;

      [SerializeField]
      public bool useValue;
      [SerializeField]
      public bool canBeUsed;

      [SerializeField]
      public int intValue;
      [SerializeField]
      public float floatValue;
      [SerializeField]
      public Vector2 vector2Value;
      [SerializeField]
      public Vector3 vector3Value;
      [SerializeField]
      public Color colorValue;
      [SerializeField]
      public Texture2D texture2dValue;

      public IOSlot(string _label = "Empty Slot", Type _type = Type.Universal, bool _useValue = true, bool _canBeUsed = true) {
        label = _label;
        type = _type;
        useValue = _useValue;
        canBeUsed = _canBeUsed;
      }
    };
    public List<IOSlot> inputSlots;
    public List<IOSlot> outputSlots;

    public Node(Type type) {
      Init(type);
    }

    public void Init(Type nodeType) {
      type = nodeType;
      name = nodeType.ToString();

      switch (nodeType) {
        case Type.Output: {
          rect = new Rect(100.0f, 100.0f, 150, 180);
          inputSlots = new List<IOSlot>();
          inputSlots.Add(new IOSlot("Diffuse", IOSlot.Type.Color));
          inputSlots.Add(new IOSlot("Normal", IOSlot.Type.Color));
          inputSlots.Add(new IOSlot("Metallic", IOSlot.Type.Float));
          inputSlots.Add(new IOSlot("Smoothness", IOSlot.Type.Float));
          inputSlots.Add(new IOSlot("Opacity", IOSlot.Type.Float));
          inputSlots.Add(new IOSlot("Emission", IOSlot.Type.Color));
          inputSlots.Add(new IOSlot("Heightmap", IOSlot.Type.Color));
          inputSlots.Add(new IOSlot("Occlusion", IOSlot.Type.Float));

          outputSlots = new List<IOSlot>();
        } break;

        case Type.ConstFloat: {
          rect = new Rect(100.0f, 100.0f, 96, 48);
          inputSlots = new List<IOSlot>();
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("", IOSlot.Type.Float));
        } break;

        case Type.ConstVector2: {
          rect = new Rect(100.0f, 100.0f, 200, 90);
          inputSlots = new List<IOSlot>();
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("", IOSlot.Type.Vector2));
          outputSlots.Add(new IOSlot("X", IOSlot.Type.Float, false));
          outputSlots.Add(new IOSlot("Y", IOSlot.Type.Float, false));
        } break;

        case Type.ConstVector3: {
          rect = new Rect(100.0f, 100.0f, 200, 96);
          inputSlots = new List<IOSlot>();
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("", IOSlot.Type.Vector3));
          outputSlots.Add(new IOSlot("X", IOSlot.Type.Float, false));
          outputSlots.Add(new IOSlot("Y", IOSlot.Type.Float, false));
          outputSlots.Add(new IOSlot("Z", IOSlot.Type.Float, false));
        } break;

        case Type.ConstColor: {
          rect = new Rect(100.0f, 100.0f, 100, 116);
          inputSlots = new List<IOSlot>();
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("", IOSlot.Type.Color));
          outputSlots.Add(new IOSlot("R", IOSlot.Type.Float, false));
          outputSlots.Add(new IOSlot("G", IOSlot.Type.Float, false));
          outputSlots.Add(new IOSlot("B", IOSlot.Type.Float, false));
          outputSlots.Add(new IOSlot("A", IOSlot.Type.Float, false));
        } break;

        case Type.Texture: {
          rect = new Rect(100.0f, 100.0f, 132, 132);
          inputSlots = new List<IOSlot>();
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("", IOSlot.Type.Texture, true, false));
          outputSlots.Add(new IOSlot("RGBA", IOSlot.Type.Color, false));
          outputSlots.Add(new IOSlot("R", IOSlot.Type.Float, false));
          outputSlots.Add(new IOSlot("G", IOSlot.Type.Float, false));
          outputSlots.Add(new IOSlot("B", IOSlot.Type.Float, false));
          outputSlots.Add(new IOSlot("A", IOSlot.Type.Float, false));
        } break;

        case Type.Time: {
          rect = new Rect(100.0f, 100.0f, 72, 54);
          inputSlots = new List<IOSlot>();
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("T", IOSlot.Type.Float, false));
        } break;

        case Type.SinTime: {
          rect = new Rect(100.0f, 100.0f, 72, 54);
          inputSlots = new List<IOSlot>();
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("T", IOSlot.Type.Float, false));
        } break;

        case Type.CosTime: {
          rect = new Rect(100.0f, 100.0f, 72, 54);
          inputSlots = new List<IOSlot>();
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("T", IOSlot.Type.Float, false));
        } break;

        case Type.Add: {
          rect = new Rect(100.0f, 100.0f, 78, 64);
          inputSlots = new List<IOSlot>();
          inputSlots.Add(new IOSlot("A", IOSlot.Type.Universal, false));
          inputSlots.Add(new IOSlot("B", IOSlot.Type.Universal, false));
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("Result", IOSlot.Type.Universal, false));
        } break;

        case Type.Subtraction: {
          rect = new Rect(100.0f, 100.0f, 78, 64);
          inputSlots = new List<IOSlot>();
          inputSlots.Add(new IOSlot("A", IOSlot.Type.Universal, false));
          inputSlots.Add(new IOSlot("B", IOSlot.Type.Universal, false));
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("Result", IOSlot.Type.Universal, false));
        } break;

        case Type.Multiply: {
          rect = new Rect(100.0f, 100.0f, 78, 64);
          inputSlots = new List<IOSlot>();
          inputSlots.Add(new IOSlot("A", IOSlot.Type.Universal, false));
          inputSlots.Add(new IOSlot("B", IOSlot.Type.Universal, false));
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("Result", IOSlot.Type.Universal, false));
        } break;

        case Type.Divide: {
          rect = new Rect(100.0f, 100.0f, 78, 64);
          inputSlots = new List<IOSlot>();
          inputSlots.Add(new IOSlot("A", IOSlot.Type.Universal, false));
          inputSlots.Add(new IOSlot("B", IOSlot.Type.Universal, false));
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("Result", IOSlot.Type.Universal, false));
        } break;

        case Type.PackVector2: {
          rect = new Rect(100.0f, 100.0f, 108, 68);
          inputSlots = new List<IOSlot>();
          inputSlots.Add(new IOSlot("X", IOSlot.Type.Float, false));
          inputSlots.Add(new IOSlot("Y", IOSlot.Type.Float, false));
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("XY", IOSlot.Type.Vector2, false));
        } break;

        case Type.PackVector3: {
          rect = new Rect(100.0f, 100.0f, 108, 80);
          inputSlots = new List<IOSlot>();
          inputSlots.Add(new IOSlot("X", IOSlot.Type.Float, false));
          inputSlots.Add(new IOSlot("Y", IOSlot.Type.Float, false));
          inputSlots.Add(new IOSlot("Z", IOSlot.Type.Float, false));
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("XYZ", IOSlot.Type.Vector3, false));
        } break;

        case Type.PackColor: {
          rect = new Rect(100.0f, 100.0f, 108, 100);
          inputSlots = new List<IOSlot>();
          inputSlots.Add(new IOSlot("R", IOSlot.Type.Float, false));
          inputSlots.Add(new IOSlot("G", IOSlot.Type.Float, false));
          inputSlots.Add(new IOSlot("B", IOSlot.Type.Float, false));
          inputSlots.Add(new IOSlot("A", IOSlot.Type.Float, false));
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("RGBA", IOSlot.Type.Color, false));
        } break;

        case Type.Lerp: {
          rect = new Rect(100.0f, 100.0f, 78, 80);
          inputSlots = new List<IOSlot>();
          inputSlots.Add(new IOSlot("A", IOSlot.Type.Universal, false));
          inputSlots.Add(new IOSlot("B", IOSlot.Type.Universal, false));
          inputSlots.Add(new IOSlot("T", IOSlot.Type.Float, false));
          outputSlots = new List<IOSlot>();
          outputSlots.Add(new IOSlot("Result", IOSlot.Type.Universal, false));
        } break;
      }
    }

    public void ShowContent() {
      EditorGUILayout.Space();

      if (inputSlots.Count > 0) {
        float areaWidth = rect.width;
        if (outputSlots.Count > 0) {
          areaWidth *= 0.5f;
        }

        GUILayout.BeginArea(new Rect(0, 24, areaWidth, rect.height - 24));
        for (int i = 0; i < inputSlots.Count; ++i) {
          GUI.color = MaterialEditor.textColor;
          EditorGUILayout.LabelField(inputSlots[i].label, MaterialEditor.nodeInputLabelStyle);
          GUI.color = Color.white;
        }
        GUILayout.EndArea();
      }

      if (outputSlots.Count > 0) {
        float startX = 0;
        float areaWidth = rect.width;
        if (inputSlots.Count > 0) {
          areaWidth *= 0.5f;
          startX = rect.width / 2.0f;
        }

        GUILayout.BeginArea(new Rect(startX, 24, areaWidth, rect.height - 24));
        for (int i = 0; i < outputSlots.Count; ++i) {
          if (!outputSlots[i].useValue) {
            GUI.color = MaterialEditor.textColor;
            EditorGUILayout.LabelField(outputSlots[i].label, MaterialEditor.nodeOutputLabelStyle);
            GUI.color = Color.white;
            continue;
          }

          switch (outputSlots[i].type) {
            case IOSlot.Type.Float: {
              GUI.color = MaterialEditor.textColor;
              outputSlots[i].floatValue = EditorGUILayout.FloatField(outputSlots[i].label, outputSlots[i].floatValue, MaterialEditor.nodeFieldStyle);
              GUI.color = Color.white;
            } break;

            case IOSlot.Type.Vector2: {
              outputSlots[i].vector2Value = EditorGUILayout.Vector2Field(outputSlots[i].label, outputSlots[i].vector2Value);
            } break;

            case IOSlot.Type.Vector3: {
              outputSlots[i].vector3Value = EditorGUILayout.Vector3Field(outputSlots[i].label, outputSlots[i].vector3Value);
            } break;

            case IOSlot.Type.Color: {
              outputSlots[i].colorValue = EditorGUILayout.ColorField(outputSlots[i].label, outputSlots[i].colorValue);
            } break;

            case IOSlot.Type.Texture: {
              outputSlots[i].texture2dValue = (Texture2D)EditorGUILayout.ObjectField(outputSlots[i].texture2dValue, typeof(Texture2D), false);
            } break;

            default: {
              EditorGUILayout.LabelField(outputSlots[i].label);
            } break;
          }
        }
        GUILayout.EndArea();
      }
    }

    public Rect GetInputSlotRect(int slot) {
      return new Rect(rect.x - 16, rect.y + 26 + ((EditorGUIUtility.singleLineHeight + 2) * slot), 12, 12);
    }
    
    public Rect GetOutputSlotRect(int slot) {
      return new Rect(rect.x + rect.width + 4, rect.y + 26 + ((EditorGUIUtility.singleLineHeight + 2) * slot), 12, 12);
    }
  }
}
